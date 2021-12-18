using FXCommon.Common;
using FXCommon.MqQueue;
using MealTicket_Web_Handler.Runner;
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
                int MsgType = newData.MsgType;
                if (MsgType == 0)
                {
                    ModifyPlateTag_Notice(newData.SendData);
                }
                else if (MsgType == 1)
                {
                    ResetPlateTag_Notice();
                }
                else if (MsgType == 2)
                {
                    ResetSharesTag_Notice();
                }

            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }

        private void ModifyPlateTag_Notice(dynamic newData) 
        {
            int market = newData.Market;
            string sharesCode = newData.SharesCode;
            List<Plate_Tag_FocusOn_Session_Info> result_FocusOn = new List<Plate_Tag_FocusOn_Session_Info>();
            List<Plate_Tag_Force_Session_Info> result_Force = new List<Plate_Tag_Force_Session_Info>();
            List<Plate_Tag_TrendLike_Session_Info> result_TrendLike = new List<Plate_Tag_TrendLike_Session_Info>();
            foreach (var item in newData.ParList)
            {
                result_FocusOn.Add(new Plate_Tag_FocusOn_Session_Info
                {
                    SharesCode = sharesCode,
                    Market = market,
                    IsFocusOn = item.IsFocusOn,
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
                        IsForce1 = force.IsForce1,
                        IsForce2 = force.IsForce2,
                        PlateId = item.PlateId,
                    });
                }
            }
            Singleton.Instance.sessionHandler.UpdateSessionPart((int)Enum_Excute_Type.Plate_Tag_FocusOn_Session, result_FocusOn);
            Singleton.Instance.sessionHandler.UpdateSessionPart((int)Enum_Excute_Type.Plate_Tag_Force_Session, result_Force);
            Singleton.Instance.sessionHandler.UpdateSessionPart((int)Enum_Excute_Type.Plate_Tag_TrendLike_Session, result_TrendLike);
        }

        private void ResetPlateTag_Notice()
        {
            Singleton.Instance.sessionHandler.UpdateSessionManual();
            PlateTagCalHelper.Calculate();
        }

        private void ResetSharesTag_Notice()
        {
            Singleton.Instance.sessionHandler.UpdateSessionManual();
            SharesTagCalHelper.Calculate();
        }
    }
}
