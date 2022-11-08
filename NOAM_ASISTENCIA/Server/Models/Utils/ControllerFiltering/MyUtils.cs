namespace NOAM_ASISTENCIA.Server.Models.Utils.ControllerFiltering
{
    public class MyUtils
    {
        public static int StringToInt(string sVal, int iDefault)
        {
            if (sVal == null)
                return iDefault;
            try
            {
                return Convert.ToInt32(sVal);
            }
            catch (Exception)
            {
                return iDefault;
            }
        }

        //convert string to date (string may be: 2000.12.31 or 31.12.2000 or 31.12.00 or 1.1.2001 or 1.1.01 - any delimiter)
        //if the string contains of additional time HH:MM then it is also used
        public static DateTime? GetDateFromString(string sDateTime)
        {
            DateTime dtRet = DateTime.Now; //

            try
            {
                string s1 = GetDateFromStringPart(ref sDateTime);
                string s2 = GetDateFromStringPart(ref sDateTime);
                string s3 = GetDateFromStringPart(ref sDateTime);
                string s4 = GetDateFromStringPart(ref sDateTime);
                string s5 = GetDateFromStringPart(ref sDateTime);
                int vD = 0;
                int vM = 0;
                int vY = 0;
                int vHH = 0;
                int vMM = 0;

                if (s1.Length != 1 && s1.Length != 2 && s1.Length != 4 ||
                    s2.Length != 1 && s2.Length != 2 ||
                    s3.Length != 2 && s3.Length != 4 ||
                    s1.Length == 4 && s3.Length == 4)
                    return null;
                if (s1.Length == 4)
                {
                    // YYYY.MM.DD
                    vY = StringToInt(s1, -1);
                    vM = StringToInt(s2, -1);
                    vD = StringToInt(s3, -1);
                }
                else
                {
                    // DD.MM.YYYY or DD.MM.YY
                    vD = StringToInt(s1, -1);
                    vM = StringToInt(s2, -1);
                    vY = StringToInt(s3, -1);
                    if (vY < 90)
                        vY += 2000;
                    else if (vY < 100)
                        vY += 1900;
                }
                if (vD < 1 || vD > 31 || vM < 1 || vM > 12 || vY < 1990 || vY > 2090)
                    return null;

                if (s4.Length > 0 && s5.Length > 0)
                {
                    vHH = StringToInt(s4, -1);
                    vMM = StringToInt(s5, -1);
                    if (vHH < 0 || vHH > 23 || vMM < 0 || vMM > 59)
                        return null;
                }
                else
                {
                    vHH = 0;
                    vMM = 0;
                }
                dtRet = new DateTime(vY, vM, vD, vHH, vMM, 0);

                return dtRet;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string GetDateFromStringPart(ref string s)
        {
            string sPart = "";
            while (!string.IsNullOrEmpty(s) && (s[0] < '0' || s[0] > '9'))
            {
                s = s.Substring(1);
            }
            while (!string.IsNullOrEmpty(s) && s[0] >= '0' && s[0] <= '9')
            {
                sPart += s[0];
                s = s.Substring(1);
            }
            return sPart;
        }

    }
}
