using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Web2App.Interfaces;
using Web2App.Models;

namespace Web2App.Services
{
    public class TsContainerService : ITsContainerService
    {
        private readonly HttpContext _httpContext;
        public TsContainerService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }
        public async Task<TSContainer> MakeTsContainer(QrCodePostModel model)
        {
            //get host name for url (qr code)
            string host = "https" + "://" + _httpContext.Request.Host.ToUriComponent();


            //Create TS Container
            var tsContainer = new TSContainer();
            tsContainer.Header = new Header("HMACSHA256");

            //Create SignableContainer
            tsContainer.SignableContainer = new SignableContainer();

            //Create Proto Info
            var protoInfo = new ProtoInfo("web2app", "1.0");

            //Create Operation Id
            string operationId = model.OperationId;

            //store your operationId in database or smth (for testing I store it in session)
            var start = ConvertDatetimeToUnixTimeStamp(DateTime.SpecifyKind(model.Start, DateTimeKind.Utc));
            var end = ConvertDatetimeToUnixTimeStamp(DateTime.SpecifyKind(model.End, DateTimeKind.Utc));

            //Create Operation Info
            var operationInfo = new OperationInfo(model.Type == OperationType.Sign ? OperationType.Sign : OperationType.Auth, operationId.ToString(), start, end);
            if (model.Assignee != null)
            {
                var assignees = model.Assignee.Split(",");
                foreach (var assignee in assignees)
                {
                    operationInfo.AddAssignee(assignee);
                }
            }

            var dataInfo = new DataInfo("SHA256");
            //Finger Print

            var clientInfo = new ClientInfo(model.ClientId, model.IconUri, model.CallBackUrl);

            tsContainer.SignableContainer.ClientInfo = clientInfo;
            tsContainer.SignableContainer.ProtoInfo = protoInfo;
            tsContainer.SignableContainer.OperationInfo = operationInfo;
            //tsContainer.SignableContainer.DataInfo = dataInfo;


            //serialize and sign signable container
            string signableJsonContainer = JsonConvert.SerializeObject(tsContainer.SignableContainer);


            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(model.SecretKey)))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    var signableContainerSha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(signableJsonContainer));

                    var hashData = hmac.ComputeHash(signableContainerSha256Bytes);
                    var base64Signature = Convert.ToBase64String(hashData);
                    tsContainer.Header.Signature = base64Signature;
                }


            }


            return tsContainer;
        }
        public static long ConvertDatetimeToUnixTimeStamp(DateTime date)
        {
            DateTime originDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date - originDate;
            return (long)Math.Floor(diff.TotalSeconds);
        }
    }
}
