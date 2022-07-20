using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;



namespace CardEncoder_453KProj
{
    public class CardEncoder_453K
    {
        //1 配置服务器地址
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CE_ConfigServer(string url);

        //2 连接设备
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_ConnectComm(string portName);

        //3 关闭连接
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_DisconnectComm();

        //4 配置发卡器
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_InitCardEncoder(string hotelInfo);

        //5 将空白卡写成酒店专用卡
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_InitCard(string hotelInfo);

        //6 停止发送空白卡操作
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_StopInitCard();

        //7 新增单条IC卡数据
        //[DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int CE_WriteCard(StringBuilder hotelInfo, int buildInfo, int floorNo, StringBuilder mac, ulong timestamp, bool allowLockOut);
        //[DllImport("u8_client", EntryPoint = "multi", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr multi(string files);
        [DllImport("CardEncoder.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_WriteCard(string hotelInfo, int buildInfo, int floorNo, string mac, ulong timestamp, bool allowLockOut);

        //8 清空IC卡数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_ClearCard(string hotelInfo);

        //9 读取IC卡中的所有数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_ReadCard(string hotelInfo,ref IntPtr hotelArray);

        //10 获取IC卡卡号
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_GetCardNo(ref IntPtr cardNumber);

        //11 蜂鸣器发声
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_Beep(int voiceLen,int interval,int voiceCount);

        //12 获取设备的版本信息
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_GetVersion(ref IntPtr versions);

        //13 发工程卡
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_InitConstructionCard();

        //14 恢复空白卡
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_DeInitCard(string hotelInfo);

        //15 写入挂失卡信息
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_CancelCard(string hotelInfo,string cardNo,ulong timestamp);

        //16 读取IC卡中的挂失相关数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_ReadCancellationInfo(string hotelInfo,ref string infoArray);

        //17 设置扇区可用性
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_SetSectors(string sectors);

        //18 读取扇区可用性
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_GetSectors(ref IntPtr sectors);

        //19 解析单个扇区的数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_ParseResData(string hotelInfo,string sectorData,bool isLowestSector,ref IntPtr hotelArray);

        //20 创建单个扇区数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_GenerateSectorData(string hotelInfo, string sectorData, bool isLowestSector, int buildNo, int floorNo,string mac,ulong timestamp,bool allowLockOut);

        //21 创建用于删除区数据的数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_GenerateClearData(string srcBytes);

        //22 获取新增挂失信息的单个扇区数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_GenerateCancelCardData(string hotelInfo,string sectorData,bool isLowestSector,string cardNo,ulong timestamp);

        //23 解析单个扇区内挂失相关数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_ParseLossData(string hotelInfo, string sectorData,bool isLowestSector,ref string infoArray);

        //24 创建工程卡块区数据
        [DllImport("CardEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CE_GenerateConstructionCardData(string blockData,string cardNo);

        private IniFile m_ObjIniFile = null;
        private string m_strIsShow = "";

        public CardEncoder_453K()
        {
            if (m_ObjIniFile == null)
            {
                m_ObjIniFile = new IniFile(@"C:\PMSPlus\" + @"lock.ini");
            }

            m_strIsShow  = m_ObjIniFile.ReadString("lockinfo", "isshow", "0");
        }

        public string MakeCard(string sourceData)
        {
            ShowMsgDlg("MakeCard");
            ShowMsgDlg("传入参数:" + sourceData);
            //CancelCard("11");
            string strFloorNo;
            string strRoomNo;
            string strStime;
            string strEtime;
            string strArrSdate;

            string strServerUrl = "";
            string strPortName = "";
            string strAllowLockOut = "";
            string strLockNo = "";
            string strHotelInfo = "";
            string strMsg = "";
            string strShectors = "";
            int iRet = 0;
            string strResult = "";  //返回值

            //解析json
            JsonTextReader objReader = new JsonTextReader(new StringReader(sourceData));
            JObject objJson = (JObject)JToken.ReadFrom(objReader);
            JToken objToken = objJson["Room_Data"];
            strFloorNo = objToken["FloorNo"].ToString();
            strRoomNo = objToken["RoomNo"].ToString();
            strStime = objToken["Stime"].ToString();
            strEtime = objToken["Etime"].ToString();
            strArrSdate = objToken["ArrSdate"].ToString();
            ShowMsgDlg("Json解析结果 " + "strFloorNo:" + strFloorNo + ",strRoomNo:" + strRoomNo + ",strStime:" + strStime + ",strEtime:" + strEtime + ",strArrSdate:" + strArrSdate);

            //读取lock文件
            strServerUrl = m_ObjIniFile.ReadString("lockinfo", "ServerUrl", "0");
            strPortName = m_ObjIniFile.ReadString("lockinfo", "PortName", "0");
            strAllowLockOut = m_ObjIniFile.ReadString("lockinfo", "AllowLockOut", "0");
            strShectors = m_ObjIniFile.ReadString("lockinfo", "Shectors", "1111111111111111");
            strLockNo = m_ObjIniFile.ReadString("pms-lock", strRoomNo, "0");
            string[] strLockNoArr = new string[] { };
            strLockNoArr = strLockNo.Split('|');
            ShowMsgDlg("读取配置文件结果 " + "ServerUrl:" + strServerUrl + ",PortName:" + strPortName + ",AllowLockOut:" + strAllowLockOut + ",Shectors:" + strShectors + ",锁号:" + strLockNoArr.Length.ToString());

            //配置服务器
            if (CE_ConfigServer(strServerUrl) == false)
            {
                strResult = ParseErrorJsonStr("301", "配置服务器失败!");
            }
            else
            {
                //获取HotelInf
                string CardNo = string.Empty;
                if (GetHotelInfo(ref strHotelInfo, ref strMsg) == false)
                {
                    strResult = ParseErrorJsonStr("302", "获取HotelInfo失败," + strMsg);
                }
                else
                {
                    //System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
                    //ulong tmSecond = (ulong)(Convert.ToDateTime(strEtime).Ticks - startTime.Ticks);   
                    //iRet = CE_WriteCard(strHotelInfo, 0, 0, "000000000000", tmSecond, true);
                    //if (iRet != 0)
                    //{
                    //    MessageBox.Show(iRet.ToString() + ParseErr(iRet) + "测试");
                    //}
                    //关掉端口
                    CE_DisconnectComm();
                    ShowMsgDlg("关掉端口成功！");
                    //连接设备
                    iRet = CE_ConnectComm(strPortName);
                    if (iRet != 0)
                    {
                        ShowMsgDlg("连接设备错误");
                        strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                    }
                    else
                    {
                        //配置发卡器
                        iRet = CE_InitCardEncoder(strHotelInfo);
                        if (iRet != 0)
                        {
                            ShowMsgDlg("配置发卡器错误");
                            strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                        }
                        else
                        {
                            //设置扇区可用性
                            ShowMsgDlg("配置文件设置的扇区：" + strShectors);
                            iRet = CE_SetSectors(strShectors);
                            ShowMsgDlg("设置扇区可用性返回值：" + iRet.ToString());
                            if (iRet != 0)
                            {
                                strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                            }
                            else
                            {
                                //读扇区可用性
                                IntPtr ipSectors = new IntPtr();
                                iRet = CE_GetSectors(ref ipSectors);
                                //ShowMsgDlg("读扇区可用性返回值:" + iRet.ToString());
                                ShowMsgDlg("读到的扇区:" + Marshal.PtrToStringAnsi(ipSectors));
                                if (iRet != 0)
                                {
                                    MessageBox.Show(ParseErr(iRet));
                                }

                                //设置为酒店专用卡
                                iRet = CE_InitCard(strHotelInfo);
                                if (iRet != 0)
                                {
                                    strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                                }
                                else
                                {
                                    iRet = CE_ClearCard(strHotelInfo);//CE_DeInitCard
                                    if (iRet != 0)
                                    {
                                        strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                                    }
                                    //else
                                    //{
                                    //    strResult = ParseErrorJsonStr("0", "销卡成功!");
                                    //}
                                   
                                    //System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
                                    //ulong lEndTime = (ulong)(Convert.ToDateTime(strEtime).Ticks - startTime.Ticks); //时间戳 精确到秒
                                    ulong lEndTime = (ulong)(DateTimeToUnixTime(Convert.ToDateTime(strEtime)));
                                    bool bAllowLockOut = true;
                                    if (strAllowLockOut == "0")
                                    {
                                        bAllowLockOut = false;
                                    }

                                    if (bAllowLockOut)
                                    {
                                        ShowMsgDlg("允许：true");
                                    }
                                    else
                                    {
                                        ShowMsgDlg("允许：false");
                                    }
                                    for (int i = 0; i < strLockNoArr.Length; i++)
                                    {
                                        strLockNo = strLockNoArr[i];
                                        string[] strArr = strLockNo.Split('-');
                                        ShowMsgDlg("锁号:" + strLockNo);
                                        int iBuildNo = Int32.Parse(strArr[0]);
                                        ShowMsgDlg("栋号:" + iBuildNo.ToString());
                                        int iFloorNo = Int32.Parse(strArr[1]);
                                        ShowMsgDlg("层号:" + iFloorNo.ToString());
                                        string strMac = strArr[2];
                                        ShowMsgDlg("锁MAC:" + strMac);
                                        iRet = CE_WriteCard(strHotelInfo, iBuildNo, iFloorNo, strMac, (ulong)lEndTime, bAllowLockOut);
                                    }
                                    
                                    if (iRet != 0)
                                    {
                                        strResult = ParseErrorJsonStr("307", ParseErr(iRet));
                                    }
                                    else
                                    {
                                        strResult = ParseErrorJsonStr("0", "制卡成功!");
                                    }
                                }
                            }

                            /*
                            //设置为酒店专用卡
                            iRet = CE_InitCard(strHotelInfo);
                            if(iRet != 0)
                            {
                                strResult = ParseErrorJsonStr("306", ParseErr(iRet));
                            }
                            else
                            {
                                string[] strArr = strLockNo.Split('-');
                                ShowMsgDlg("锁号:" + strLockNo);
                                int iBuildNo = Int32.Parse(strArr[0]);
                                ShowMsgDlg("栋号:" + iBuildNo.ToString());
                                int iFloorNo = Int32.Parse(strArr[1]);
                                ShowMsgDlg("层号:" + iFloorNo.ToString());
                                string strMac = strArr[2];
                                ShowMsgDlg("锁MAC:" + strMac);
                                //System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
                                //ulong lEndTime = (ulong)(Convert.ToDateTime(strEtime).Ticks - startTime.Ticks); //时间戳 精确到秒
                                ulong lEndTime = (ulong)(DateTimeToUnixTime(Convert.ToDateTime(strEtime)));
                                bool bAllowLockOut = true;
                                if (strAllowLockOut == "0")
                                {
                                    bAllowLockOut = false;
                                }

                                iRet = CE_WriteCard(strHotelInfo, iBuildNo, iFloorNo, strMac, (ulong)lEndTime, bAllowLockOut);
                                if (iRet != 0)
                                {
                                    strResult = ParseErrorJsonStr("307", ParseErr(iRet));
                                }
                                else
                                {
                                    strResult = ParseErrorJsonStr("0", "制卡成功!");
                                }
                            }
                            */
                        }
                    }
                }
            }

            CE_Beep(100, 100, 4);
            //关闭连接
            CE_DisconnectComm();
            return strResult;
        }

        public string ReadCard(string sourceData)
        {
            ShowMsgDlg("ReadCard");

            string strRoomNo;
            //string strStime;
            string strEtime;

            string strServerUrl = "";
            string strPortName = "";
            string strAllowLockOut = "";
            string strHotelInfo = "";
            string strMsg = "";
            int iRet = 0;
            string strResult = "";  //返回值

            //读取lock文件
            strServerUrl = m_ObjIniFile.ReadString("lockinfo", "ServerUrl", "0");
            strPortName = m_ObjIniFile.ReadString("lockinfo", "PortName", "0");
            strAllowLockOut = m_ObjIniFile.ReadString("lockinfo", "AllowLockOut", "0");
            ShowMsgDlg("读取配置文件结果 " + "ServerUrl:" + strServerUrl + ",PortName:" + strPortName + ",AllowLockOut:" + strAllowLockOut);

            //配置服务器
            if (CE_ConfigServer(strServerUrl) == false)
            {
                strResult = ParseErrorJsonStr("401", "配置服务器失败!");
            }
            else
            {
                //获取HotelInfo
                if (GetHotelInfo(ref strHotelInfo, ref strMsg) == false)
                {
                    strResult = ParseErrorJsonStr("402", "获取HotelInfo失败," + strMsg);
                }
                else
                {
                    //关掉端口
                    CE_DisconnectComm();
                    ShowMsgDlg("关掉端口成功！");
                    //连接设备
                    iRet = CE_ConnectComm(strPortName);
                    if (iRet != 0)
                    {
                        ShowMsgDlg("连接设备失败!" + iRet.ToString());
                        strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                    }
                    else
                    {
                        //配置发卡器
                        iRet = CE_InitCardEncoder(strHotelInfo);
                        ShowMsgDlg("配置发卡器!" + iRet.ToString());
                        if (iRet != 0)
                        {
                            strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                        }
                        else
                        {
                            //设置为酒店专用卡
                            iRet = CE_InitCard(strHotelInfo);
                            ShowMsgDlg("设置为酒店专用卡!" + iRet.ToString());
                            if (iRet != 0)
                            {
                                strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                            }
                            else
                            {
                                IntPtr objPtr = new IntPtr();
                                iRet = CE_ReadCard(strHotelInfo, ref objPtr);
                                ShowMsgDlg("读卡!" + iRet.ToString());
                                if (iRet != 0)
                                {
                                    strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                                }
                                else
                                {
                                    ShowMsgDlg("准备解析读卡的数据");
                                    string strCardInfo = Marshal.PtrToStringAnsi(objPtr);
                                    ShowMsgDlg("读卡数据:" + strCardInfo);
                                    //解析josn
                                    JObject joRead = JObject.Parse(strCardInfo);
                                    string strCount = joRead["count"].ToString();
                                    string strHotelArray = joRead["hotelArray"].ToString();
                                    if (strCount == "0")
                                    {
                                        strResult = ParseErrorJsonStr("0", "空白卡!");
                                    }
                                    else
                                    {
                                        string strInfoArr = "";
                                        JArray jaHotelArray = (JArray)JsonConvert.DeserializeObject(strHotelArray);
                                        //for (int i = 0; i < jaHotelArray.Count; i++)
                                        //{
                                        //    JObject joBodyData = (JObject)jaHotelArray[i];
                                        //    string strBuildNo = joBodyData["buildNo"].ToString();
                                        //    string strFloorNo = joBodyData["floorNo"].ToString();
                                        //    string strMac = joBodyData["mac"].ToString();
                                        //    string strTimestamp = joBodyData["timestamp"].ToString();
                                        //    string strLockNo = strBuildNo + "-" + strFloorNo + "-" + strMac;
                                        //    ShowMsgDlg("锁号:" + strLockNo);
                                        //    strRoomNo = m_ObjIniFile.ReadString("lock-pms", strLockNo, "0");
                                        //    strEtime = UnixTimeToDateTime(Convert.ToInt32(strTimestamp)).ToString("yyyy-MM-dd HH:mm:ss");

                                        //    strInfoArr = strInfoArr + "  房号:" + strRoomNo + " 离店时间:" + strEtime;
                                        //}
                                        JObject joBodyData = (JObject)jaHotelArray[0];
                                        string strBuildNo = joBodyData["buildNo"].ToString();
                                        string strFloorNo = joBodyData["floorNo"].ToString();
                                        string strMac = joBodyData["mac"].ToString();
                                        string strTimestamp = joBodyData["timestamp"].ToString();
                                        string strLockNo = strBuildNo + "-" + strFloorNo + "-" + strMac;
                                        ShowMsgDlg("锁号:" + strLockNo);
                                        strRoomNo = m_ObjIniFile.ReadString("lock-pms", strLockNo, "0");
                                        strEtime = UnixTimeToDateTime(Convert.ToInt32(strTimestamp)).ToString("yyyy-MM-dd HH:mm:ss");

                                        strInfoArr = strInfoArr + "  房号:" + strRoomNo + " 离店时间:" + strEtime;

                                        strResult = ParseErrorJsonStr("0", "读卡成功!" + strInfoArr);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            CE_Beep(100, 100, 4);
            //关闭连接
            CE_DisconnectComm();
            return strResult;
        }

        public string CancelCard(string sourceData)
        {
            ShowMsgDlg("CancelCard");

            string strServerUrl = "";
            string strPortName = "";
            string strAllowLockOut = "";
            string strHotelInfo = "";
            string strMsg = "";
            int iRet = 0;
            string strResult = "";  //返回值

            //读取lock文件
            strServerUrl = m_ObjIniFile.ReadString("lockinfo", "ServerUrl", "0");
            strPortName = m_ObjIniFile.ReadString("lockinfo", "PortName", "0");
            strAllowLockOut = m_ObjIniFile.ReadString("lockinfo", "AllowLockOut", "0");
            ShowMsgDlg("读取配置文件结果 " + "ServerUrl:" + strServerUrl + ",PortName:" + strPortName + ",AllowLockOut:" + strAllowLockOut);

            //配置服务器
            if (CE_ConfigServer(strServerUrl) == false)
            {
                strResult = ParseErrorJsonStr("501", "配置服务器失败!");
            }
            else
            {
                //获取HotelInfo
                if (GetHotelInfo(ref strHotelInfo, ref strMsg) == false)
                {
                    strResult = ParseErrorJsonStr("502", "获取HotelInfo失败," + strMsg);
                }
                else
                {
                    //关掉端口
                    CE_DisconnectComm();
                    ShowMsgDlg("关掉端口成功！");
                    //连接设备
                    iRet = CE_ConnectComm(strPortName);
                    if (iRet != 0)
                    {
                        strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                    }
                    else
                    {
                        //配置发卡器
                        iRet = CE_InitCardEncoder(strHotelInfo);
                        if (iRet != 0)
                        {
                            strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                        }
                        else
                        {
                            //设置为酒店专用卡
                            iRet = CE_InitCard(strHotelInfo);
                            if (iRet != 0)
                            {
                                strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                            }
                            else
                            {
                                iRet = CE_ClearCard(strHotelInfo);//CE_DeInitCard
                                if (iRet != 0)
                                {
                                    strResult = ParseErrorJsonStr(iRet.ToString(), ParseErr(iRet));
                                }
                                else
                                {
                                    strResult = ParseErrorJsonStr("0", "销卡成功!");
                                }
                            }
                        }
                    }
                }
            }

            CE_Beep(100, 100, 4);
            //关闭连接
            CE_DisconnectComm();
            return strResult;
        }

        //获取hotelInfo
        private bool GetHotelInfo(ref string strHotelInfo, ref string strMsg)
        {
            try
            {
                //读取lock文件
                string strHotelInfoUrl = m_ObjIniFile.ReadString("lockinfo", "HotelInfoUrl", "0"); 
                string strClientId = m_ObjIniFile.ReadString("lockinfo", "ClientId", "0");
                string strClientSecret = m_ObjIniFile.ReadString("lockinfo", "ClientSecret", "0");
                string strCfgHotelInfo = m_ObjIniFile.ReadString("lockinfo", "HotelInfo", "");
                string strCfgHotelInfoTime = m_ObjIniFile.ReadString("lockinfo", "HotelInfoTime", "");
                ShowMsgDlg("读取配置文件结果 " + "HotelInfoUrl:" + strHotelInfoUrl + ",ClientId:" + strClientId + ",ClientSecret:" + strClientSecret + ",HotelInfo:" + strCfgHotelInfo + ",HotelInfoTime:" + strCfgHotelInfoTime);

                if (strCfgHotelInfo != "")
                {
                    DateTime dtNowTime = DateTime.Parse(DateTime.Now.ToString());
                    DateTime dtCfgTime = DateTime.Parse(strCfgHotelInfoTime);
                    System.TimeSpan tsTime = dtNowTime - dtCfgTime;  //两个时间相减 默认得到的是 两个时间之间的天数 得到：365.00:00:00 
                    double dHours = tsTime.TotalHours;
                    if (dHours < 12)
                    {
                        strHotelInfo = strCfgHotelInfo;
                        return true;
                    }
                }

                //当前时间 10位
                long lCurrentTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

                string strUrl = strHotelInfoUrl + "clientId=" + strClientId + "&clientSecret=" + strClientSecret + "&date=" + lCurrentTime.ToString();
                ShowMsgDlg("获取hotelInfo的Url:" + strUrl);
                //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(strUrl);
                //request.Method = "GET";
                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //Stream stream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(stream);
                //string retString = reader.ReadToEnd();
                string retString = HttpGet(strUrl);
                ShowMsgDlg("获取hotelInfo响应数据:" + retString);
                //解析josn
                JObject joRead = JObject.Parse(retString);
                strHotelInfo = joRead["hotelInfo"].ToString();
                //string strErrCode = joRead["errcode"].ToString();
                //if (strErrCode == "0")
                //{
                //    strHotelInfo = joRead["hotelInfo"].ToString();
                //}
                //else
                //{
                //    strMsg = joRead["errmsg"].ToString();
                //}

                m_ObjIniFile.WriteString("lockinfo", "HotelInfo", strHotelInfo);
                m_ObjIniFile.WriteString("lockinfo", "HotelInfoTime", DateTime.Now.ToString());

                return true;
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                return false;
            }
        }

        //将c# DateTime时间格式转换为Unix时间戳格式   13位
        private long ConvertDateTimeToLong(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位 
            ShowMsgDlg("时间戳:" + t.ToString());
            return t;
        }

        /// <summary>
        /// 时间戳转换成标准时间
        /// </summary>
        /// <param name="timeStamp">时间戳</param>
        /// <returns></returns>
        private DateTime ConvertToDateTime(string timeStamp)
        {
            DateTime dtTime = DateTime.Now;
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            dtTime = dtStart.Add(toNow);
            return dtTime;
        }

        //解析错误码
        private string ParseErr(int iIndex)
        {
            string strErrInfo = "";
            switch (iIndex)
            {
                case 1:
                    {
                        strErrInfo = "操作失败!";
                    }
                    break;
                case 2:
                    {
                        strErrInfo = "接口参数错误!";
                    }
                    break;
                case 3:
                    {
                        strErrInfo = "指令发送错误或设备未连接!";
                    }
                    break;
                case 4:
                    {
                        strErrInfo = "操作错误或强行终止!";
                    }
                    break;
                case 5:
                    {
                        strErrInfo = "指令错误!";
                    }
                    break;
                case 10:
                    {
                        strErrInfo = "服务器地址未配置!";
                    }
                    break;
                case 11:
                    {
                        strErrInfo = "服务器请求失败!";
                    }
                    break;
                case 12:
                    {
                        strErrInfo = "服务器返回错误!";
                    }
                    break;
                case 13:
                    {
                        strErrInfo = "hotelInfo无效";
                    }
                    break;
                case 101:
                    {
                        strErrInfo = "设备返回其它错误!";
                    }
                    break;
                case 102:
                    {
                        strErrInfo = "超时无IC卡!";
                    }
                    break;
                case 104:
                    {
                        strErrInfo = "IC卡存储空间不足!";
                    }
                    break;
                case 105:
                    {
                        strErrInfo = "数据解密失败!";
                    }
                    break;
                case 106:
                    {
                        strErrInfo = "IC卡解密失败!";
                    }
                    break;
                case 109:
                    {
                        strErrInfo = "酒店ID未配置!";
                    }
                    break;
                case 201:
                    {
                        strErrInfo = "配置秘钥失败!";
                    }
                    break;
                case 202:
                    {
                        strErrInfo = "配置卡秘钥失败!";
                    }
                    break;
                case 203:
                    {
                        strErrInfo = "配置酒店信息失败!";
                    }
                    break;
                case 301:
                    {
                        strErrInfo = "数据额解析的其他错误!";
                    }
                    break;
                case 304:
                    {
                        strErrInfo = "扇区空间不足!";
                    }
                    break;
                case 305:
                    {
                        strErrInfo = "秘钥解密失败或未配置";
                    }
                    break;
                case 307:
                    {
                        strErrInfo = "IC卡数据不存在!";
                    }
                    break;
                case 420:
                    {
                        strErrInfo = "数据未正常返回!";
                    }
                    break;
                default:
                    {
                        strErrInfo = "未知错误!";
                    }
                    break;
            }

            return strErrInfo;
        }

        private string ParseErrorJsonStr(string strCode,string strMsg)
        {
            return  "{\"code\":" + "\"" + strCode + "\",\"msg\":" + "\"" + strMsg + "\"}";
        }

        private void ShowMsgDlg(string strMsg)
        {
            if ("0" == m_strIsShow)
            {
                MessageBox.Show(strMsg);
            }
        }

        /// <summary>  
        /// 获取Assembly的运行路径  
        /// </summary>  
        ///<returns></returns>  
        private string GetAssemblyPath()
        {
            string _CodeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

            _CodeBase = _CodeBase.Substring(8, _CodeBase.Length - 8);    // 8是file:// 的长度  

            string[] arrSection = _CodeBase.Split(new char[] { '/' });

            string _FolderPath = "";
            for (int i = 0; i < arrSection.Length - 1; i++)
            {
                _FolderPath += arrSection[i] + "/";
            }

            return _FolderPath;
        }

        /// <summary>
        /// 将dateTime格式转换为Unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private int DateTimeToUnixTime(DateTime dateTime)
        {
            return (int)(dateTime - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        /// <summary>
        /// 将Unix时间戳转换为dateTime格式
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private DateTime UnixTimeToDateTime(int time)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException("time is out of range");

            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(time);
        }
        public static string HttpGet(string url)
        {
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
