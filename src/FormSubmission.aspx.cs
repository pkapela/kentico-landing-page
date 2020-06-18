using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

public partial class FormSubmission : System.Web.UI.Page {
    static string GLOBAL_KenticoConn = ConfigurationManager.ConnectionStrings["CMSConnectionString"].ConnectionString;

	public FormSubmission() {
        Page.Items["CsrfProtectionDisabledOnPage"] = true;
    }

    protected void Page_Load(object sender, EventArgs e) {
        string studylevel = Request.Form.Get("inputDegree").Trim();
        string email = Request.Form.Get("inputEmail").Trim();
        string lname = Request.Form.Get("inputLname").Trim();
        string fname = Request.Form.Get("inputFname").Trim();
        string programID = Request.Form.Get("inputProgram").Trim();
        string enyear = Request.Form.Get("inputRadio").Trim();

        int statusID;
        string errMsg = "";

        if (IsValidFormData(studylevel, email, lname, fname, programID, enyear, out errMsg)) {
            if (InsertFormData(studylevel, email, lname, fname, programID, enyear)) {
                statusID = 0;  // success
            } else {
                statusID = 1;  // update failure
            }
        } else {
            statusID = 2;  // invalid data
        }

        Response.Write(statusID + ";" + errMsg);

        if (statusID == 0) {
            // everything is good
        } else {
            if (statusID == 1) {
                WriteLog(studylevel, 'E', "INSERT FAILED", errMsg);
            } else if (statusID == 2) {
                // WriteLog(studylevel, 'W', "VALIDATION FAILED", errMsg);
            } else {
                WriteLog(studylevel, 'E', "OTHER FAILURE", errMsg);
            }
        }
    }

    private bool IsValidFormData(string studylevel, string email, string lname, string fname, string programID, string enyear, out string errMsg) {
        const int MAX_STR_LEN = 75;
        bool isValid = true;
        errMsg = "";

        if (studylevel == "A" || studylevel == "B") {
            // ok
        } else {
            errMsg += "Invalid studylevel: " + studylevel + ".<br>" + Environment.NewLine;
            WriteLog(studylevel, 'E', "studylevel validation", errMsg);
            isValid = false;
        }

        if (!isValid) {
            return false;
        }

        if (email.Length == 0) {
            errMsg += "Email is missing.<br>" + Environment.NewLine;
            isValid = false;
        }

        if (email.Length > MAX_STR_LEN) {
            errMsg += "Email is too long.<br>" + Environment.NewLine;
            isValid = false;
        }

        if (lname.Length == 0) {
            errMsg += "Last Name is missing.<br>" + Environment.NewLine;
            isValid = false;
        }

        if (lname.Length > MAX_STR_LEN) {
            errMsg += "Last Name is too long.<br>" + Environment.NewLine;
            isValid = false;
        }

        if (fname.Length == 0) {
            errMsg += "First Name is missing.<br>" + Environment.NewLine;
            isValid = false;
        }

        if (fname.Length > MAX_STR_LEN) {
            errMsg += "First Name is too long.<br>" + Environment.NewLine;
            isValid = false;
        }

        if (enyear.Length != 4) {
            errMsg += "Entrance Year is incorrect.<br>" + Environment.NewLine;
            isValid = false;
        }

        int tmpID;
        if (!int.TryParse(programID, out tmpID)) {
            errMsg += "Please select a Program.<br>" + Environment.NewLine;
            isValid = false;
        }

        if (!isValid) {
            return false;
        }

        if (!IsValidEmail(email)) {
            errMsg += "Invalid email address: " + "'" + email + "'<br>" + Environment.NewLine;
            return false;
        }

        DataTable emailSearch = GetInquiryByEmail(studylevel, email, programID);
        bool emailExists = emailSearch.Rows.Count == 1;

        if (emailExists) {
            errMsg += "Email address already exists.<br>" + Environment.NewLine;
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string strIn) {
        bool invalidEmail = false;

        if (String.IsNullOrEmpty(strIn)) {
            return false;
        }

        try {
            strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
        } catch (Exception e) {
            return false;
        }

        try {
            return Regex.IsMatch(strIn,
                  @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                  RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        } catch (RegexMatchTimeoutException) {
            return false;
        }
    }

    private string DomainMapper(Match match) {
        IdnMapping idn = new IdnMapping();
        string domainName = match.Groups[2].Value;
        domainName = idn.GetAscii(domainName);

        return match.Groups[1].Value + domainName;
    }

    private bool InsertFormData(string studylevel, string email, string lname, string fname, string programID, string enyear) {

        string programName = ProgramLookUp(studylevel, programID);

        try {
            using (SqlConnection cn = new SqlConnection(GLOBAL_KenticoConn)) {
                cn.Open();

                using (SqlCommand saveData = new SqlCommand()) {
                    saveData.Connection = cn;

                    string sqlText;
                    string sqlAddText1 = "Email, LastName, FirstName, ProgramID, ProgramName, EntryYear";
                    string sqlAddText2 = "@EmailParam, @LastNameParam, @FirstNameParam, @ProgramIDParam, @ProgramNameParam, @EntryYearParam";

                    saveData.Parameters.Add("@EmailParam", SqlDbType.VarChar).Value = email;
                    saveData.Parameters.Add("@LastNameParam", SqlDbType.VarChar).Value = lname;
                    saveData.Parameters.Add("@FirstNameParam", SqlDbType.VarChar).Value = fname;
                    saveData.Parameters.Add("@ProgramIDParam", SqlDbType.Int).Value = programID;
                    saveData.Parameters.Add("@ProgramNameParam", SqlDbType.VarChar).Value = programName;
                    saveData.Parameters.Add("@EntryYearParam", SqlDbType.VarChar).Value = enyear;

                    sqlAddText1 += ", ";
                    sqlAddText2 += ", ";
                    sqlAddText1 += "ItemCreatedWhen";
                    sqlAddText2 += "SYSDATETIME()";

                    sqlAddText1 += ", ";
                    sqlAddText2 += ", ";
                    sqlAddText1 += "ItemModifiedWhen";
                    sqlAddText2 += "SYSDATETIME()";

                    if (studylevel == "A") {
                        sqlText = "INSERT INTO ProgTableA (" + sqlAddText1 + ") VALUES (" + sqlAddText2 + ")";
                    } else if (studylevel == "B") {
                        sqlText = "INSERT INTO ProgTableB (" + sqlAddText1 + ") VALUES (" + sqlAddText2 + ")";
                    } else {
                        throw new Exception("Invalid studylevel: " + studylevel);
                    }
                    saveData.CommandText = sqlText;
                    int rowsUpdated = saveData.ExecuteNonQuery();
                }
            }

            string addMsg =
            "l: " + studylevel + Environment.NewLine +
            "e: " + email + Environment.NewLine +
            "l: " + lname + Environment.NewLine +
            "f: " + fname + Environment.NewLine +
            "p: " + programID + Environment.NewLine +
            "n: " + programName + Environment.NewLine;

            WriteLog(studylevel, 'I', "SUCCESS", addMsg);
            return true;

        } catch (Exception e) {
            string errmsg = "Error saving data" + Environment.NewLine;
            errmsg += "email: '" + email + "'" + Environment.NewLine;
            errmsg += e.Message + Environment.NewLine;
            errmsg += e.StackTrace + Environment.NewLine;

            WriteLog(studylevel, 'E', "INSERT FAILURE", errmsg);
            return false;
        }
    }

    private string ProgramLookUp(string studylevel, string programID) {

        string programName = "";
          
        switch (programID) {
            case "0":
                programName = "Donec laoreet";
                break;
            case "1":
                programName = "Aenean placerat lectus";
                break;
            case "2":
                programName = "Aenean tortor quam";
                break;
            case "3":
                programName = "Ut egestas ante";
                break;
            case "4":
                programName = "Praesent pretium";
                break;
            case "5":
                programName = "Nunc maximus varius";
                break;
            default:
                programName = "_blank";
                break;
        }
        return programName;
    }

    private static DataTable GetInquiryByEmail(string studylevel, string email, string programID) {
        using (SqlConnection cn = new SqlConnection(GLOBAL_KenticoConn)) {
            cn.Open();

            using (SqlCommand getRecByEmail = new SqlCommand()) {
                getRecByEmail.Connection = cn;

                if (studylevel == "A") {
                    getRecByEmail.CommandText = "SELECT Email FROM ProgTableA WHERE Email = @EmailParam AND ProgramID = @ProgramIDParam";
                } else if (studylevel == "B") {
                    getRecByEmail.CommandText = "SELECT Email FROM ProgTableB WHERE Email = @EmailParam AND ProgramID = @ProgramIDParam";
                } else {
                    throw new Exception("Invalid studylevel: " + studylevel);
                }

                getRecByEmail.Parameters.Add("@EmailParam", SqlDbType.VarChar).Value = email;
                getRecByEmail.Parameters.Add("@ProgramIDParam", SqlDbType.VarChar).Value = programID;

                using (SqlDataReader admReader = getRecByEmail.ExecuteReader()) {
                    DataTable tbl = new DataTable();
                    tbl.Load(admReader);
                    return tbl;
                }
            }
        }
    }

    private static void WriteLog(string studylevel, char severity, string EventCode, string LogMessage) {

        using (SqlConnection cn = new SqlConnection(GLOBAL_KenticoConn)) {
            cn.Open();

            using (SqlCommand saveLog = new SqlCommand()) {
                saveLog.Connection = cn;
                saveLog.CommandText = "INSERT INTO CMS_EventLog (EventType, EventTime, Source, EventCode, EventDescription) VALUES (@EventTypeParam, SYSDATETIME(), @SourceParam, @EventCodeParam, @EventDescriptionParam)";
                saveLog.Parameters.Add("@EventTypeParam", SqlDbType.VarChar).Value = severity;
                saveLog.Parameters.Add("@SourceParam", SqlDbType.VarChar).Value = "Adm_" + studylevel.ToUpper() + "_Inquiry";
                saveLog.Parameters.Add("@EventDescriptionParam", SqlDbType.VarChar).Value = LogMessage;
                saveLog.Parameters.Add("@EventCodeParam", SqlDbType.VarChar).Value = EventCode;
                saveLog.ExecuteNonQuery();
            }
        }
    }
}
