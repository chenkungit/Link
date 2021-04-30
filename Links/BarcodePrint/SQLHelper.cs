using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Data;
using System.Collections;
using System.Data.SqlClient;


namespace Links
{
    public class SQLHelper
    {
        const string connString = @"";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_strSqlText"></param>
        /// <returns></returns>
        public static int uExecuteNonQuery(string _strSqlText)
        {
            int _iReturn = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(_strSqlText, conn);
                    _iReturn = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch { }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return _iReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_strSqlText"></param>
        /// <returns></returns>
        public static object uExecuteScalar(string _strSqlText)
        {
            object _objReturn = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(_strSqlText, conn);
                    _objReturn = cmd.ExecuteScalar();
                    conn.Close();
                }
                catch { }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return _objReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_strSqlText"></param>
        /// <param name="_strTableName"></param>
        /// <returns></returns>
        public static DataTable uExecuteDataTable(string _strSqlText, string _strTableName)
        {
            DataSet _ds = new DataSet();
            DataTable _dt = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(_strSqlText, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    _ds.Clear();
                    da.Fill(_ds, _strTableName);

                    _dt = _ds.Tables[_strTableName];
                    conn.Close();
                }
                catch { }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return _dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_strSqlText"></param>
        /// <param name="_strTableName"></param>
        /// <returns></returns>
        public static DataSet uExecuteDataSet(string _strSqlText, string _strTableName)
        {
            DataSet _ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(_strSqlText, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);

                    _ds.Clear();
                    da.Fill(_ds, _strTableName);

                    conn.Close();
                }
                catch { }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return _ds;
        }

        /// <summary>
        /// 保存境外数据方法
        /// </summary>
        /// <param name="_countryType">六大洲编号</param>
        /// <param name="_continents">六大洲名称</param>
        /// <param name="_provinceId">国家ID</param>
        /// <param name="_provinceName">国家名称</param>
        /// <param name="_confirmedCount">累计确诊</param>
        /// <param name="_confirmedIncr">新增确诊</param>
        /// <param name="_curedCount">累计治愈</param>
        /// <param name="_curedIncr">新增治愈</param>
        /// <param name="_currentConfirmedCount">现有确诊</param>
        /// <param name="_currentConfirmedIncr">现有确诊较昨日</param>
        /// <param name="_dateId">日期</param>
        /// <param name="_deadCount">累计死亡</param>
        /// <param name="_deadIncr">新增死亡</param>
        public static void uSaveRecordForeign(string _countryType, string _continents, string _provinceId, string _provinceName, string _confirmedCount, string _confirmedIncr, string _curedCount, string _curedIncr, string _currentConfirmedCount, string _currentConfirmedIncr, string _dateId, string _deadCount, string _deadIncr)
        {
            if (string.IsNullOrEmpty(_confirmedCount)) _confirmedCount = "0";
            if (string.IsNullOrEmpty(_confirmedIncr)) _confirmedIncr = "0";
            if (string.IsNullOrEmpty(_curedCount)) _curedCount = "0";
            if (string.IsNullOrEmpty(_curedIncr)) _curedIncr = "0";
            if (string.IsNullOrEmpty(_currentConfirmedCount)) _currentConfirmedCount = "0";
            if (string.IsNullOrEmpty(_currentConfirmedIncr)) _currentConfirmedIncr = "0";
            if (string.IsNullOrEmpty(_deadCount)) _deadCount = "0";
            if (string.IsNullOrEmpty(_deadIncr)) _deadIncr = "0";


            string mysql = string.Format(@"SELECT COUNT(1) FROM nCovDataForeign WHERE countryType = {0} AND continents = '{1}' AND provinceId = {2} AND provinceName = '{3}' AND dateId = '{4}'", _countryType, _continents, _provinceId, _provinceName, _dateId);

            object _objCheck = uExecuteScalar(mysql);

            if (_objCheck == null || string.IsNullOrEmpty(_objCheck.ToString()) || Convert.ToInt32(_objCheck) == 0)
            {
                mysql = string.Format(@"INSERT INTO nCovDataForeign(
countryType,continents,provinceId,provinceName,confirmedCount,
confirmedIncr,curedCount,curedIncr,currentConfirmedCount,currentConfirmedIncr,
dateId,deadCount,deadIncr)
VALUES
(
{0},'{1}',{2},'{3}',{4},
{5},{6},{7},{8},{9},
'{10}',{11},{12}
)",
   _countryType, _continents, _provinceId, _provinceName, _confirmedCount,
  _confirmedIncr, _curedCount, _curedIncr, _currentConfirmedCount, _currentConfirmedIncr,
   _dateId, _deadCount, _deadIncr
   );
            }
            else
            {
                mysql = string.Format(@"UPDATE nCovDataForeign SET
confirmedCount={5},confirmedIncr={6},curedCount={7},curedIncr={8},
currentConfirmedCount={9},currentConfirmedIncr={10},deadCount={11},deadIncr={12}
WHERE countryType = {0} AND continents = '{1}' AND provinceId = {2} AND provinceName = '{3}' AND dateId = '{4}'",
 _countryType, _continents, _provinceId, _provinceName, _dateId,
 _confirmedCount, _confirmedIncr, _curedCount, _curedIncr,
 _currentConfirmedCount, _currentConfirmedIncr, _deadCount, _deadIncr
       );
            }
            uExecuteNonQuery(mysql);
        }

        /// <summary>
        /// 保存国内数据方法
        /// </summary>
        /// <param name="_Farea">地区</param>
        /// <param name="_Fdate">日期</param>
        /// <param name="_AddConfirm">新增确诊</param>
        /// <param name="_TotalConfirm">累计确诊</param>
        /// <param name="_TotalSuspect">现有疑似</param>
        /// <param name="_TotalDead">累计死亡</param>
        /// <param name="_TotalHeal">累计治愈</param>
        /// <param name="_TotalSevere">现有重症</param>
        /// <param name="_Dinical">新增临床</param>
        /// <param name="_TotalDinical">累计临床</param>
        /// <param name="_outlands">境外新增</param>
        /// <param name="_TotalOutlands">境外累计</param>
        /// <param name="_RelieveAsymptomatic">无症状解除</param>
        /// <param name="_ConfirmAsymptomatic">无症状转确诊</param>
        public static void uSaveRecordnCovData(string _Farea, string _Fdate, string _AddConfirm, string _TotalConfirm, string _TotalSuspect, string _TotalDead, string _TotalHeal, string _TotalSevere, string _Dinical, string _TotalDinical, string _outlands, string _TotalOutlands, string _RelieveAsymptomatic, string _ConfirmAsymptomatic)
        {
            try
            {
                string Farea = _Farea.Trim();
                string Fdate = _Fdate;
                string AddConfirm = _AddConfirm.Trim().Equals("") ? "0" : _AddConfirm.Trim();
                string TotalConfirm = _TotalConfirm.Trim().Equals("") ? "0" : _TotalConfirm.Trim();
                string TotalSuspect = _TotalSuspect.Trim().Equals("") ? "0" : _TotalSuspect.Trim();
                string TotalDead = _TotalDead.Trim().Equals("") ? "0" : _TotalDead.Trim();
                string TotalHeal = _TotalHeal.Trim().Equals("") ? "0" : _TotalHeal.Trim();
                string TotalSevere = _TotalSevere.Trim().Equals("") ? "0" : _TotalSevere.Trim();
                string Dinical = _Dinical.Trim().Equals("") ? "0" : _Dinical.Trim();

                string TotalDinical = _TotalDinical.Trim().Equals("") ? "0" : _TotalDinical.Trim();
                string outlands = _outlands.Trim().Equals("") ? "0" : _outlands.Trim();
                string TotalOutlands = _TotalOutlands.Trim().Equals("") ? "0" : _TotalOutlands.Trim();
                string RelieveAsymptomatic = _RelieveAsymptomatic.Trim().Equals("") ? "0" : _RelieveAsymptomatic.Trim();
                string ConfirmAsymptomatic = _ConfirmAsymptomatic.Trim().Equals("") ? "0" : _ConfirmAsymptomatic.Trim();

                string mysql = string.Format("SELECT COUNT(1) FCOUNT FROM nCovData WHERE Farea = '{0}' AND Fdate = '{1}'", Farea, Fdate);

                object _objCheck = uExecuteScalar(mysql);

                if (_objCheck == null || string.IsNullOrEmpty(_objCheck.ToString()) || Convert.ToInt32(_objCheck) == 0)
                {
                    mysql = string.Format(@"insert into nCovData(
Farea,Fdate,AddConfirm,TotalConfirm,
TotalSuspect,TotalDead,TotalHeal,TotalSevere,
Dinical,TotalDinical,outlands,TotalOutlands,
RelieveAsymptomatic,ConfirmAsymptomatic
) 
values (
'{0}','{1}',{2},{3},
{4},{5},{6},{7},
{8},{9},{10},{11},
{12},{13}
)
",
 Farea, Fdate, AddConfirm, TotalConfirm,
 TotalSuspect, TotalDead, TotalHeal, TotalSevere,
 Dinical, TotalDinical, outlands, TotalOutlands,
 RelieveAsymptomatic, ConfirmAsymptomatic
 );
                }
                else
                {
                    mysql = string.Format(@"UPDATE nCovData SET AddConfirm={2},TotalConfirm={3},
TotalSuspect={4},TotalDead={5},TotalHeal={6},TotalSevere={7},Dinical={8},TotalDinical={9},outlands={10},TotalOutlands={11} ,RelieveAsymptomatic={12} ,ConfirmAsymptomatic={13} 
WHERE Farea = '{0}' AND Fdate = '{1}'",
 Farea, Fdate, AddConfirm, TotalConfirm,
 TotalSuspect, TotalDead, TotalHeal, TotalSevere,
 Dinical, TotalDinical, outlands, TotalOutlands,
 RelieveAsymptomatic, ConfirmAsymptomatic
 );
                }
                uExecuteNonQuery(mysql);
            }
            catch
            {

            }
        }


        /// <summary>
        /// 保存省内数据方法
        /// </summary>
        /// <param name="_provinceShortName">省名称</param>
        /// <param name="_confirmedCount">累计确诊</param>
        /// <param name="_confirmedIncr">新增确诊</param>
        /// <param name="_curedCount">累计治愈</param>
        /// <param name="_curedIncr">新增治愈</param>
        /// <param name="_currentConfirmedCount">现有确诊</param>
        /// <param name="_currentConfirmedIncr">现有确诊较昨日</param>
        /// <param name="_dateId">日期</param>
        /// <param name="_deadCount">累计死亡</param>
        /// <param name="_deadIncr">新增死亡</param>
        public static void uSaveRecordProvince(string _provinceShortName, string _confirmedCount, string _confirmedIncr, string _curedCount, string _curedIncr, string _currentConfirmedCount, string _currentConfirmedIncr, string _dateId, string _deadCount, string _deadIncr)
        {
            try
            {
                string provinceShortName = _provinceShortName.Trim();
                string confirmedCount = _confirmedCount.Trim().Equals("") ? "0" : _confirmedCount.Trim();
                string confirmedIncr = _confirmedIncr.Trim().Equals("") ? "0" : _confirmedIncr.Trim();
                string curedCount = _curedCount.Trim().Equals("") ? "0" : _curedCount.Trim();
                string curedIncr = _curedIncr.Trim().Equals("") ? "0" : _curedIncr.Trim();
                string currentConfirmedCount = _currentConfirmedCount.Trim().Equals("") ? "0" : _currentConfirmedCount.Trim();
                string currentConfirmedIncr = _currentConfirmedIncr.Trim().Equals("") ? "0" : _currentConfirmedIncr.Trim();

                string dateId = _dateId;
                string deadCount = _deadCount.Trim().Equals("") ? "0" : _deadCount.Trim();
                string deadIncr = _deadIncr.Trim().Equals("") ? "0" : _deadIncr.Trim();

                string mysql = string.Format("SELECT COUNT(1) FCOUNT FROM nCovDataProvince WHERE provinceShortName = '{0}' AND dateId = '{1}'", provinceShortName, dateId);

                object _objCheck = uExecuteScalar(mysql);

                if (_objCheck == null || string.IsNullOrEmpty(_objCheck.ToString()) || Convert.ToInt32(_objCheck) == 0)
                {
                    mysql = string.Format(@"insert into nCovDataProvince(
provinceShortName,confirmedCount,confirmedIncr,curedCount,curedIncr,
currentConfirmedCount,currentConfirmedIncr,dateId,deadCount,deadIncr
) 
values (
'{0}',{1},{2},{3},{4},
{5},{6},'{7}',{8},{9}
)
",
 provinceShortName, confirmedCount, confirmedIncr, curedCount, curedIncr,
 currentConfirmedCount, currentConfirmedIncr, dateId, deadCount, deadIncr
 );
                }
                else
                {
                    mysql = string.Format(@"UPDATE nCovDataProvince SET confirmedCount={2},confirmedIncr={3},
curedCount={4},curedIncr={5},currentConfirmedCount={6},currentConfirmedIncr={7},deadCount={8},deadIncr={9}
WHERE provinceShortName = '{0}' AND dateId = '{1}'",
 provinceShortName, dateId, confirmedCount, confirmedIncr,
 curedCount, curedIncr, currentConfirmedCount, currentConfirmedIncr,
 deadCount, deadIncr
 );
                }
                uExecuteNonQuery(mysql);
            }
            catch
            {

            }
        }


        /// <summary>
        /// 保存城市数据方法
        /// </summary>
        /// <param name="_provinceShortName">省名称</param>
        /// <param name="_confirmedCount">累计确诊</param>
        /// <param name="_confirmedIncr">新增确诊</param>
        /// <param name="_curedCount">累计治愈</param>
        /// <param name="_curedIncr">新增治愈</param>
        /// <param name="_currentConfirmedCount">现有确诊</param>
        /// <param name="_currentConfirmedIncr">现有确诊较昨日</param>
        /// <param name="_dateId">日期</param>
        /// <param name="_deadCount">累计死亡</param>
        /// <param name="_deadIncr">新增死亡</param>
        public static void uSaveRecordCity(string _provinceID, string _provinceName, string _locationId, string _cityName, string _confirmedCount, string _confirmedIncr, string _curedCount, string _curedIncr, string _currentConfirmedCount, string _currentConfirmedIncr, string _dateId, string _deadCount, string _deadIncr, string _suspectedCount, string _suspectedIncr)
        {
            try
            {
                string provinceID = _provinceID.Trim();
                string provinceName = _provinceName.Trim();
                string locationId = _locationId.Trim();
                string cityName = _cityName.Trim();
                string confirmedCount = _confirmedCount.Trim().Equals("") ? "0" : _confirmedCount.Trim();
                string confirmedIncr = _confirmedIncr.Trim().Equals("") ? "0" : _confirmedIncr.Trim();
                string curedCount = _curedCount.Trim().Equals("") ? "0" : _curedCount.Trim();
                string curedIncr = _curedIncr.Trim().Equals("") ? "0" : _curedIncr.Trim();
                string currentConfirmedCount = _currentConfirmedCount.Trim().Equals("") ? "0" : _currentConfirmedCount.Trim();
                string currentConfirmedIncr = _currentConfirmedIncr.Trim().Equals("") ? "0" : _currentConfirmedIncr.Trim();

                string dateId = _dateId;
                string deadCount = _deadCount.Trim().Equals("") ? "0" : _deadCount.Trim();
                string deadIncr = _deadIncr.Trim().Equals("") ? "0" : _deadIncr.Trim();
                string suspectedCount = _suspectedCount.Trim().Equals("") ? "0" : _suspectedCount.Trim();
                string suspectedIncr = _suspectedIncr.Trim().Equals("") ? "0" : _suspectedIncr.Trim();

                string mysql = string.Format("SELECT COUNT(1) FCOUNT FROM nCovDataCity WHERE provinceID = '{0}' AND provinceName = '{1}' AND locationId = '{2}' AND cityName = '{3}' AND dateId = '{4}'", provinceID, provinceName, locationId, cityName, dateId);

                object _objCheck = uExecuteScalar(mysql);

                if (_objCheck == null || string.IsNullOrEmpty(_objCheck.ToString()) || Convert.ToInt32(_objCheck) == 0)
                {
                    mysql = string.Format(@"insert into nCovDataCity(
provinceID,provinceName,locationId,cityName,confirmedCount,
confirmedIncr,curedCount,curedIncr,currentConfirmedCount,currentConfirmedIncr,
dateId,deadCount,deadIncr,suspectedCount,suspectedIncr
) 
values (
'{0}','{1}','{2}','{3}',{4},
{5},{6},{7},{8},{9},
'{10}',{11},{12},{13},{14}
)
",
provinceID,provinceName,locationId,cityName,confirmedCount,
confirmedIncr,curedCount,curedIncr,currentConfirmedCount,currentConfirmedIncr,
dateId,deadCount,deadIncr,suspectedCount,suspectedIncr
);
                }
                else
                {
                    mysql = string.Format(@"UPDATE nCovDataCity SET 
confirmedCount={5},confirmedIncr={6},curedCount={7},curedIncr={8},currentConfirmedCount={9},
currentConfirmedIncr={10},deadCount={11},deadIncr={12},suspectedCount={13},suspectedIncr={14}
WHERE provinceID = '{0}' AND provinceName = '{1}' AND locationId = '{2}' AND cityName = '{3}' AND dateId = '{4}'",
provinceID, provinceName, locationId, cityName, dateId,
confirmedCount,confirmedIncr,curedCount,curedIncr,currentConfirmedCount,
currentConfirmedIncr,deadCount,deadIncr,suspectedCount,suspectedIncr
 );
                }
                uExecuteNonQuery(mysql);
            }
            catch
            {

            }
        }
    }
}
