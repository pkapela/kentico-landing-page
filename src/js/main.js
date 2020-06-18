(function () {
  "use strict";
  window.addEventListener("load", function () {
    var form = document.getElementById("AdmFormID");
    form.addEventListener("submit", function (event) {
      event.preventDefault();

      var formData = getFormData(form);
      var errors = validationRoutine(formData);

      if ((typeof errors === "undefined") && (formData.inputProgram) != -1 && (formData.inputRadio != -1)) {

        /* Your Ajax goes here */
        /* Form data is successfully validated */

        $.ajax({
          type: "POST",
          url: "SaveAdmissionsInquiry.aspx",
          data: $.param(formData),
          success: function (result) {
            formClearance(form);
            if (result.charAt(0) === '0') {
              $("#modalOutput").html("<div><div style=\"float: left; width: 17%;\" class=\"column left\"><img class=\"img-fluid\" src=\"img/check-mark-128x128.png\" style=\"width: 64px; height: 64px; margin-right: 15px;\" /></div><div style=\"float: left; width: 75%;\" class=\"column right\"><p>Thank you for your interest! We will contact you soon.</p></div><div>");
              $("#modalLabel").text("Confirmation");
              $("#exampleModal").modal("show");
            }
            else if (result.charAt(0) === '2') {
              $("#modalOutput").html("<div><div style=\"float: left; width: 17%;\" class=\"column left\"><img class=\"img-fluid\" src=\"img/check-mark-128x128.png\" style=\"width: 64px; height: 64px; margin-right: 15px;\" /></div><div style=\"float: left; width: 80%;\" class=\"column right\"><p>Your email address is already in our records.</p></div><div>");
              $("#modalLabel").text("Thank You");
              $("#exampleModal").modal("show");
            }
            else {
              $("#modalOutput").html("<div><div style=\"float: left; width: 17%;\" class=\"column left\"><img class=\"img-fluid\" src=\"img/error-128x128.png\" style=\"width: 64px; height: 64px; margin-right: 15px;\" /></div><div style=\"float: left; width: 80%;\" class=\"column right\"><p>There was an error during processing your request at the server. Please reload the page and try again.</p></div><div>");
              $("#modalLabel").text("Submission Failure");
              $("#exampleModal").modal("show");
            }
          },
          error: function (exception) {
            formClearance(form);
            $("#modalOutput").html("<div><div style=\"float: left; width: 17%;\" class=\"column left\"><img class=\"img-fluid\" src=\"img/error-128x128.png\" style=\"width: 64px; height: 64px; margin-right: 15px;\" /></div><div style=\"float: left; width: 80%;\" class=\"column right\"><p>There was an error connecting to the server. Please reload the page and try again.</p></div><div>");
            $("#modalLabel").text("Submission Failure");
            $("#exampleModal").modal("show");
          }
        });

      } else {
        if (typeof errors !== "undefined") {
          if (typeof errors.inputEmail !== "undefined") {
            $("#inputEmail + div").addClass("fade-in");
            $("#inputEmail + div").html(errors.inputEmail[0]);
          } else {
            $("#inputEmail + div").html("&zwnj;");
          }
          if (typeof errors.inputFname !== "undefined") {
            $("#inputFname + div").addClass("fade-in");
            $("#inputFname + div").html(errors.inputFname[0]);
          } else {
            $("#inputFname + div").html("&zwnj;");
          }
          if (typeof errors.inputLname !== "undefined") {
            $("#inputLname + div").addClass("fade-in");
            $("#inputLname + div").html(errors.inputLname[0]);
          } else {
            $("#inputLname + div").html("&zwnj;");
          }
          if (formData.inputProgram == -1) {
            $("select#program").css("border-color", "red");
            $("select#program + div").addClass("fade-in");
            $("select#program + div").html("Please make a selection");
          } else {
            $("select#program").css("border-color", "#CCC");
            $("select#program + div").html("&zwnj;");
          }
          if (formData.inputRadio == -1) {
            $("select#entry-year").css("border-color", "red");
            $("select#entry-year + div").addClass("fade-in");
            $("select#entry-year + div").html("Please make a selection");
          } else {
            $("select#entry-year").css("border-color", "#CCC");
            $("select#entry-year + div").html("&zwnj;");
          }
        } else {
          $("#email + div").html("&zwnj;");
          $("#fname + div").html("&zwnj;");
          $("#lname + div").html("&zwnj;");
          if (formData.inputProgram == -1) {
            $("select#program").css("border-color", "red");
            $("select#program + div").addClass("fade-in");
            $("select#program + div").html("Please make a selection");
          } else {
            $("select#program").css("border-color", "#CCC");
            $("select#program + div").html("&zwnj;");
          }
          if (formData.inputRadio == -1) {
            $("select#entry-year").css("border-color", "red");
            $("select#entry-year + div").addClass("fade-in");
            $("select#entry-year + div").html("Please make a selection");
          } else {
            $("select#entry-year").css("border-color", "#CCC");
            $("select#entry-year + div").html("&zwnj;");
          }
        }
      }

    }, false);
  }, false);

  function validationRoutine(formData) {
    var constraints = {
      inputEmail: {
        email: true,
        email: {
          message: "^Email has not a valid format"
        }
      },
      inputFname: {
        length: {
          minimum: 1,
          maximum: 40,
          message: "^First name cannot be blank"
        },
        format: {
          pattern: "[a-z0-9]+",
          flags: "i",
          message: "^First name can only contain A-Z and 0-9 characters"
        }
      },
      inputLname: {
        length: {
          minimum: 1,
          maximum: 40,
          message: "^Last name cannot be blank"
        },
        format: {
          pattern: "[a-z0-9]+",
          flags: "i",
          message: "^Last name can only contain A-Z and 0-9 characters"
        }
      }
    };

    return validate(formData, constraints);
  }

  function formClearance(form) {
    $("input[type=email][name=inputEmail]").val("");
    $("input[type=text][name=inputFname]").val("");
    $("input[type=text][name=inputLname]").val("");
    $("#program").val("-1");
    $("#entry-year").val("-1");

    $("#inputEmail + div").html("&zwnj;");
    $("#inputFname + div").html("&zwnj;");
    $("#inputLname + div").html("&zwnj;");

    $("select#program").css("border-color", "#CCC");
    $("select#program + div").html("&zwnj;");

    $("select#entry-year").css("border-color", "#CCC");
    $("select#entry-year + div").html("&zwnj;");
  }

  function getFormData(form) {
    var formValues = {};
    formValues.inputEmail = $("input[type=email][name=inputEmail]").val();
    formValues.inputFname = $("input[type=text][name=inputFname]").val();
    formValues.inputLname = $("input[type=text][name=inputLname]").val();

    formValues.inputProgram = $("#program").val();
    formValues.inputRadio = $("#entry-year").val();

    if ($("#" + "submit-form-phd").length == 0) {
      formValues.inputDegree = 'A';
    } else {
      formValues.inputDegree = 'B';
    }

    return formValues;
  }
}());
