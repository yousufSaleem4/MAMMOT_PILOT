<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="PlusCP.Externals.Registration" %>

<!DOCTYPE html>
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css">
<link href="../Content/css/all.css" rel="stylesheet" />
<link href="../Content/css/_all-skins.min.css" rel="stylesheet" />
<link href="../Content/css/AdminLTE.min.css" rel="stylesheet" />
<link href="../Content/css/bootstrap.min.css" rel="stylesheet" />
<link href="../Content/css/all.css" rel="stylesheet" />
<link href="../Content/css/Site.css" rel="stylesheet" />
<script src="../Scripts/jquery-3.3.1.js"></script>
<script src="../Scripts/jquery.min.js"></script>
<script src="../Scripts/bootstrap.min.js"></script>
<script src="../Scripts/adminlte.js"></script>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">
    <title>SRM Portal</title>

      <style>
       body {
    background-color: #f4f7f9;
    font-family: system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", "Noto Sans", "Liberation Sans", Arial, sans-serif !important;
}
.card-container {
    max-width: 800px;
    margin: 100px auto;
    display: flex;
    border-radius: 10px;
    overflow: hidden;
    box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);

}

/* Left Panel (Password Guidelines) */
.left-panel {
    background-color: #003B59;
    color: white;
    padding: 30px;
    width: 40%;
    text-align: left; /* Ensures text is left-aligned */
}
.left-panel h4 {
    font-size: 20px;
    margin-bottom: 15px;
}
.left-panel p {
    font-size: 14px;
    margin: 5px 0;
}

/* Right Panel (Form) */
.right-panel {
    background: white;
    padding: 30px;
    width: 60%;
}
.right-panel h4 {
    font-size: 22px;
    font-weight: bold;
    margin-bottom: 20px;
}

/* Input fields */
.form-control {
    border-radius: 8px;
    height: 45px;
    padding-left: 15px;
    font-size: 14px;
}

/* Password field alignment */
.password-container {
    position: relative;
}
.password-container .toggle-password {
    position: absolute;
    right: 15px;
    top: 66%;
    transform: translateY(-50%);
    cursor: pointer;
    color: #003B59;
}

/* Submit Button */
.btn-submit {
    background: #003B59;
    color: white;
    width: 100%;
    border-radius: 8px;
    padding: 12px;
    font-size: 16px;
    transition: 0.3s;
    margin-top: 15px;
}
.btn-submit:hover {
   background-color: transparent; /* Make background transparent on hover */
            border-color: #003B59; /* Dark border color on hover */
            color: #003B59; /* Dark font color on hover */
}

/* Captcha Box */
.captcha-text {
    font-weight: bold;
    font-size: 18px;
    background-color: #e0ffd1;
    color: #325FAB;
    width: 100%;
    text-align: center;
    border-radius: 8px;
    border: none;
    padding: 10px;
}

/* Password Validation */
.invalid {
    color: red;
}
.valid {
    color: lightgreen;
}

/* Responsive Design */
@media (max-width: 768px) {
    .card-container {
        flex-direction: column;
    }
    .left-panel, .right-panel {
        width: 100%;
        text-align: center;
    }
    .password-container .toggle-password {
        right: 10px;
    }
}

    </style>

</head>
<body runat="server">
    <div class="header">
        <a href="https://collablly.com/" target="_blank">
      <img src="/Content/Images/Collablly.gif" style="margin-left: 25px; margin-top: 10px; width: auto; height:40px;  margin-bottom: 3px;" />
</a>
    </div>
   <div class="card-container d-flex" style="padding-top:20px;">
        <!-- Left Side: Password Instructions -->
        <div class="left-panel">
            <h4>Password Guidelines 🔒</h4>
            <p id="letter" class="invalid">✔ A lowercase letter</p>
            <p id="capital" class="invalid">✔ A capital (uppercase) letter</p>
            <p id="number" class="invalid">✔ A number</p>
            <p id="length" class="invalid">✔ Minimum 8 characters (Max 15)</p>
            <p id="specialChar" class="invalid">✔ A special character (!@#$%^&*)</p>
            <p>📌 Tip: Use a mix of letters, numbers, and symbols for a stronger password!</p>
        </div>

        <!-- Right Side: Form Controls -->
        <div class="right-panel">
            <h4 class="text-center">Create Account</h4>
            <form>
                <div class="mb-3">
                    <label class="form-label">First Name</label>
                    <input type="text" class="form-control" maxlength="16" id="firstname">
                </div>
                <div class="mb-3" style="margin-top: 5px;">
                    <label class="form-label">Last Name</label>
                    <input type="text" class="form-control" maxlength="16" id="lastname">
                </div>
                <div class="mb-3 password-container" style="margin-top: 5px;">
                    <label class="form-label">Password</label>
                    <input type="password" class="form-control" maxlength="16" id="newPwd">
                    <i class="fas fa-eye toggle-password" onclick="togglePasswordVisibility('newPwd')"></i>
                </div>
                <div class="mb-3 password-container" style="margin-top: 5px;">
                    <label class="form-label">Confirm Password</label>
                    <input type="password" class="form-control" maxlength="16" id="confirmPwd">
                    <i class="fas fa-eye toggle-password" onclick="togglePasswordVisibility('confirmPwd')"></i>
                </div>
              
                <button type="button" id="btndone" class="btn btn-submit">Submit</button>
                <p class="text-center mt-3 text-danger" id="lblMsg"></p>
            </form>
        </div>
    </div>
    <asp:Label ID="lblsignid" runat="server" ForeColor="White"></asp:Label>
    <div class="tryagain">
     <%--   <span class="text-red"><b>Your request has expired.</b></span>--%>
    </div>
</body>
</html>

<script>
    window.onload = GenerateCaptcha;
    //----For User Request----//
    var signId = '';

    //----END-----//

    //---For Password Handling----//
    var isLowerCase;
    var isUpperCase;
    var isNumber;
    var isMaxLenth;
    var isSpecialChar;
    var letter = document.getElementById("letter");
    var capital = document.getElementById("capital");
    var number = document.getElementById("number");
    var length = document.getElementById("length");
    var specialChar = document.getElementById("specialChar");

    var newPwd = document.getElementById("newPwd");

    $("#newPwd").on('keypress', function () {
        if (event.keyCode == 32) {
            return false;
        }
    });

    $("#newPwd").on('keyup', function () {

        validatePassword();
    });

    function validatePassword() {
        // Validate lowercase letters
        var lowerCaseLetters = /[a-z]/g;
        if (newPwd.value.match(lowerCaseLetters)) {
            letter.classList.remove("invalid");
            letter.classList.add("valid");
            isLowerCase = true;
        } else {
            isLowerCase = false;
            letter.classList.remove("valid");
            letter.classList.add("invalid");
        }

        //  // Validate capital letters
        var upperCaseLetters = /[A-Z]/g;
        if (newPwd.value.match(upperCaseLetters)) {
            capital.classList.remove("invalid");
            capital.classList.add("valid");
            isUpperCase = true;
        } else {
            isUpperCase = false
            capital.classList.remove("valid");
            capital.classList.add("invalid");
        }

        // Validate numbers
        var numbers = /[0-9]/g;
        if (newPwd.value.match(numbers)) {
            number.classList.remove("invalid");
            number.classList.add("valid");
            isNumber = true;
        } else {
            isNumber = false;
            number.classList.remove("valid");
            number.classList.add("invalid");
        }

        // Validate length
        if (newPwd.value.length >= 8) {
            length.classList.remove("invalid");
            length.classList.add("valid");
            isMaxLenth = true;
        } else {
            isMaxLenth = false;
            length.classList.remove("valid");
            length.classList.add("invalid");
        }

        var specialChars = /[?=.*[!#$%&?@]/;
        if (newPwd.value.match(specialChars)) {
            specialChar.classList.remove("invalid");
            specialChar.classList.add("valid");
            isSpecialChar = true;
        } else {
            isSpecialChar = false;
            specialChar.classList.remove("valid");
            specialChar.classList.add("invalid");
        }
    }

    //----END----//
    function togglePasswordVisibility(fieldId) {
        var field = document.getElementById(fieldId);
        var icon = field.nextElementSibling;
        if (field.type === "password") {
            field.type = "text";
            icon.classList.remove("fa-eye");
            icon.classList.add("fa-eye-slash");
        } else {
            field.type = "password";
            icon.classList.remove("fa-eye-slash");
            icon.classList.add("fa-eye");
        }
    }

    //---- For Validating User Input-----//
    function PasswordIsValid() {

        if (isLowerCase == false) {
            document.getElementById("message").style.display = "block";
            return false
        }

        if (isUpperCase == false) {
            document.getElementById("message").style.display = "block";
            return false
        }

        if (isNumber == false) {
            document.getElementById("message").style.display = "block";
            return false
        }

        if (isMaxLenth == false) {
            document.getElementById("message").style.display = "block";
            return false
        }
        if (isSpecialChar == false) {
            document.getElementById("message").style.display = "block";
            return false
        }
        return true;

    }

    $("#btndone").click(function () {

        var retCaptcha;
        $('#lblMsg').empty();
        var firstName = $("#firstname").val();
        var lastName = $("#lastname").val();
        var newPwd = $("#newPwd").val();
        var confirmPwd = $('#confirmPwd').val();
        if (firstName == '') {

            $('#lblMsg').empty();
            $('#lblMsg').append('Enter First Name.');
            return;
        }
        if (lastName == '') {
            $('#lblMsg').empty();
            $('#lblMsg').append('Enter Last Name.');

            return;
        }
        if (newPwd == '') {
            $('#lblMsg').empty();
            $('#lblMsg').append('Enter password.');
            return;
        }

        if (confirmPwd == '') {
            $('#lblMsg').empty();
            $('#lblMsg').append('Enter confirm password.');
            return;
        }


        if (newPwd != confirmPwd) {
            $('#lblMsg').empty();
            $('#lblMsg').append('Password does not match.');
            return;
        }
    
            var newPwd = $("#newPwd").val();
            var confirmPwd = $('#confirmPwd').val();
            var token = '<%= Request.QueryString["id"]%>';
            $.ajax({
                type: 'POST',
                url: '/Home/CreateUser',
                data: { firstName: firstName, lastName: lastName, newPwd: newPwd, confirmPwd: confirmPwd, token: token },
                success: function (data) {
                    $('#lblMsg').empty();
                    $("#newPwd").val = '';
                    $('#confirmPwd').val = '';
                    $('#lblMsg').append(data);
                    if (data == "User created successfully") {
                        $('#lblMsg').removeClass('SeaGreenColor');
                        window.location.href = '/Home/Login';
                    }

                    else
                        $('#lblMsg').removeClass('SeaGreenColor');
                },
                fail: function () {
                    alert('fail');
                },
                error: function (r) {
                    alert('error');
                }

            });
        
    });

    /* Function to Generat Captcha */

    $('#txtCaptcha').bind("cut copy paste", function (e) {
        e.preventDefault();
    });


    function GenerateCaptcha() {
        var chr1 = Math.ceil(Math.random() * 10) + '';
        var chr2 = Math.ceil(Math.random() * 10) + '';
        var chr3 = Math.ceil(Math.random() * 10) + '';

        var str = new Array(3).join().replace(/(.|$)/g, function () { return ((Math.random() * 36) | 0).toString(36)[Math.random() < .4 ? "toString" : "toLowerCase"](); });
        var captchaCode = str + chr1 + '' + chr2 + '' + chr3;
        document.getElementById("txtCaptcha").value = captchaCode
    }

    /* Validating Captcha Function */
    function ValidCaptcha() {
        var str1 = removeSpaces(document.getElementById('txtCaptcha').value);
        var str2 = removeSpaces(document.getElementById('txtCompare').value);

        if (str2.length == 0) {
            $('#lblMsg').empty();
            $('#lblMsg').append('Please, enter a valid captcha.');
            return false;
        }

        if (str1 != str2) {
            $('#lblMsg').empty();
            $('#lblMsg').append('Please, enter a valid captcha.');
            return false;
        }


        if (str1 == str2)
            return true;

    }



    /* Remove spaces from Captcha Code */
    function removeSpaces(string) {
        return string.split(' ').join('');
    }
    //----- END-------//
</script>
