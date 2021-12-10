using FXCommon.Common;
using FXCommon.MqQueue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MealTicket_Web_Handler.SessionHandler;

namespace MealTicket_Web_Handler
{
    public class SharesPlateTagMQHandler : MQTask
    {
        public SharesPlateTagMQHandler(string _hostName, int _port, string _userName, string _password, string _virtualHost) : base(_hostName, _port, _userName, _password, _virtualHost)
        {
        }

        public override void ReceivedExecute(string data)
        {
            try
            {
                var newData = JsonConvert.DeserializeObject<dynamic>(data);
                int market = newData.Market;
                string sharesCode = newData.SharesCode;
                List<Plate_Tag_FocusOn_Session_Info> result_FocusOn = new List<Plate_Tag_FocusOn_Session_Info>();
                List<Plate_Tag_Force_Session_Info> result_Force = new List<Plate_Tag_Force_Session_Info>();
                List<Plate_Tag_TrendLike_Session_Info> result_TrendLike = new List<Plate_Tag_TrendLike_Session_Info>();
                foreach (var item in newData.ParList)
                {
                    result_FocusOn.Add(new Plate_Tag_FocusOn_Session_Info
                    {
                        SharesCode= sharesCode,
                        Market= market,
                        IsFocusOn= item.IsFocusOn,
                        PlateId = item.PlateId,
                    });
                    result_TrendLike.Add(new Plate_Tag_TrendLike_Session_Info
                    {
                        SharesCode = sharesCode,
                        Market = market,
                        IsTrendLike = item.IsTrendLike,
                        PlateId = item.PlateId,
                    });
                    foreach (var force in item.ForceList)
                    {
                        result_Force.Add(new Plate_Tag_Force_Session_Info
                        {
                            SharesCode = sharesCode,
                            Market = market,
                            Type = force.ForceType,
                            IsForce1= force.IsForce1,
                            IsForce2= force.IsForce2,
                            PlateId = item.PlateId,
                        });
                    }
                }
                Singleton.Instance.sessionHandler.SetSessionWithLock(Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString(),(int)Enum_Excute_Type.Plate_Tag_FocusOn_Session, result_FocusOn);
                Singleton.Instance.sessionHandler.SetSessionWithLock(Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString(), (int)Enum_Excute_Type.Plate_Tag_Force_Session, result_Force);
                Singleton.Instance.sessionHandler.SetSessionWithLock(Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString(), (int)Enum_Excute_Type.Plate_Tag_TrendLike_Session, result_TrendLike);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
