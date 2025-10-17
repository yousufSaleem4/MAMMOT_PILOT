using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace PlusCP.Models
{
    public class POMonitoringDashbord
    {
        cDAL oDAL = new cDAL(cDAL.ConnectionType.ACTIVE);

        // 🟢 Query 1
        public List<Hashtable> GetRemainingSummary()
        {
            string sql = @"
SELECT 
    Vendor_Name,
    COUNT(*) AS TotalPOLinesRemaining,
    SUM(COALESCE(PODetail_XOrderQty, 0) - COALESCE(Calculated_ArrivedQty, 0)) AS TotalRemainingQty,
    SUM(
        (COALESCE(PODetail_XOrderQty, 0) - COALESCE(Calculated_ArrivedQty, 0))
        * COALESCE(PODetail_UnitCost, 0)
    ) AS TotalCostRemaining
FROM [dbo].[PODetail]
WHERE COALESCE(PODetail_XOrderQty, 0) > COALESCE(Calculated_ArrivedQty, 0) 
GROUP BY Vendor_Name
ORDER BY Vendor_Name;
";
            DataTable dt = oDAL.GetData(sql);
            return cCommon.ConvertDtToHashTable(dt);
        }

        // 🟢 Query 2
        public List<Hashtable> GetBuyerEpicorSummary()
        {
            string sql = @"
SELECT  
    (SELECT COUNT(*) 
     FROM [dbo].[PODetail] PD 
     INNER JOIN [SRM].[BuyerPO] BPO  
         ON PD.POHeader_PONum = BPO.PONum
        AND PD.PODetail_POLine = BPO.[LineNo]
        AND PD.PORel_PORelNum = BPO.RelNo
    ) AS BuyerCommCount,

    (SELECT COUNT(*) 
     FROM [dbo].[PODetail]
    ) AS TotalEpicorPOLine;
";
            DataTable dt = oDAL.GetData(sql);
            return cCommon.ConvertDtToHashTable(dt);
        }

        // 🟢 Query 3
        public List<Hashtable> GetCompletionSummary()
        {
            string sql = @"
WITH LatestVendorComm AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY PONo ORDER BY CreatedOn DESC) AS rn
    FROM [SRM].[VendorCommunication]
)
SELECT
    (SELECT COUNT(*) 
     FROM [SRM].[BuyerPO] B
     JOIN LatestVendorComm V 
         ON B.GUID = V.GUID
         AND V.PONo = CONCAT(B.PONum, '-', B.[LineNo], '-', B.RelNo)
         AND V.rn = 1
     JOIN [dbo].[PODetail] P 
         ON P.POHeader_PONum = B.PONum
         AND P.PODetail_POLine = B.[LineNo]
         AND P.PORel_PORelNum = B.RelNo
     WHERE ISNULL(ROUND(P.PODetail_XOrderQty, 2), 0) = ISNULL(ROUND(P.Calculated_ReceivedQty, 2), 0)
    ) AS CompletedBySRM,
    (SELECT COUNT(*) 
     FROM [dbo].[PODetail] P
     WHERE ISNULL(ROUND(P.PODetail_XOrderQty, 2), 0) = ISNULL(ROUND(P.Calculated_ReceivedQty, 2), 0)
       AND NOT EXISTS (
           SELECT 1 
           FROM [SRM].[BuyerPO] B
           WHERE B.PONum = P.POHeader_PONum
             AND B.[LineNo] = P.PODetail_POLine
             AND B.RelNo = P.PORel_PORelNum
       )
    ) AS CompletedByEpicor;
";
            DataTable dt = oDAL.GetData(sql);
            return cCommon.ConvertDtToHashTable(dt);
        }

        // 🟢 Query 4
        public List<Hashtable> GetOnTimeSummary()
        {
            string sql = @"
SELECT
    Vendor_Name,
    COUNT(*) AS TotalPOs,
    SUM(CASE 
            WHEN Calculated_ArrivedDate IS NOT NULL 
                 AND Calculated_ArrivedDate <= Calculated_DueDate 
            THEN 1 ELSE 0 END) AS OnTimePOs,
    SUM(CASE 
            WHEN Calculated_ArrivedDate IS NULL 
                 OR Calculated_ArrivedDate > Calculated_DueDate 
            THEN 1 ELSE 0 END) AS DuePOs,
    CASE 
        WHEN COUNT(*) = 0 THEN 0
        ELSE ROUND(
             CAST(SUM(CASE 
                        WHEN Calculated_ArrivedDate IS NOT NULL 
                             AND Calculated_ArrivedDate <= Calculated_DueDate 
                        THEN 1 ELSE 0 END) AS DECIMAL(10,2))
             / COUNT(*) * 100, 2)
    END AS PercentageOnTime
FROM [dbo].[PODetail]
GROUP BY Vendor_Name
ORDER BY Vendor_Name;
";
            DataTable dt = oDAL.GetData(sql);
            return cCommon.ConvertDtToHashTable(dt);
        }
    }
}
