using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Web2App.Enums;
using Web2App.Interfaces;
using Web2App.Models;

namespace Web2App.Controllers
{
    public class QrCodeModel
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public string OperationId { get; set; }
        //test
    }
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        private readonly ITsContainerService _tsContainerService;
        private readonly IQrGenerator _qrGenerator;
        private readonly IApplicationDbContext _context;
        private readonly string _baseUrl;
        public HomeController(ILogger<HomeController> logger,
                            ITsContainerService tsContainerService,
                            IQrGenerator qrGenerator,
                            IApplicationDbContext context)
        {
            _logger = logger;
            _tsContainerService = tsContainerService;
            _qrGenerator = qrGenerator;
            _context = context;

        }

        public IActionResult Index(string operationId)
        {

            if (operationId == null)
                return RedirectToAction("QrCodeGenerate", "Home");

            string fileName = null;
            bool result = ApplicationData.QrCodes.TryGetValue(operationId, out fileName);

            if (result)
                return View(new QrCodeModel()
                {
                    FileName = fileName,
                    OperationId = operationId
                });
            else
                return RedirectToAction("QrCodeGenerate", "Home");

        }

        [HttpGet]
        public IActionResult QrCodeGenerate()
        {
            return View(new QrCodePostModel());
        }

        [HttpPost]
        public async Task<IActionResult> QrCodeGenerate(QrCodePostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //make ts container
            var tsContainer = await _tsContainerService.MakeTsContainer(model);

            //serialize full jsoncontainer
            string fullJsonContainer = JsonConvert.SerializeObject(tsContainer);

            string base64JsonData = Convert.ToBase64String(Encoding.UTF8.GetBytes(fullJsonContainer));

            //make url for getting file with tsquery params
            var url = "https" + "://" + HttpContext.Request.Host.ToUriComponent() + "/Home/GetFile/?tsquery=" + base64JsonData;

            //generate qr code
            var generatedQrFileName = await _qrGenerator.GenerateQr(url);

            var operationId = tsContainer.SignableContainer.OperationInfo.OperationId;

            bool qrCodeExsistResult = ApplicationData.QrCodes.TryGetValue(operationId, out string r);
            bool postedOperationResult = ApplicationData.PostedOperations.TryGetValue(operationId, out CallbackPostModel a);

            if (qrCodeExsistResult)
            {
                ApplicationData.QrCodes.Remove(operationId);
            }

            if (postedOperationResult)
            {
                ApplicationData.QrCodes.Remove(operationId);
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp", "test.pdf");

            using (FileStream reader = new FileStream(path, FileMode.Open))
            {
                var buffer = new byte[reader.Length];
                await reader.ReadAsync(buffer, 0, buffer.Length);

                path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", operationId + ".pdf");
                using (FileStream filestream = new FileStream(path, FileMode.Create))
                {
                    await filestream.WriteAsync(buffer, 0, buffer.Length);
                }

            }






            ApplicationData.QrCodes.Add(operationId, generatedQrFileName);
            ApplicationData.PostedOperations.Add(operationId, null);


            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();


            await _context.LogAsync(new SimaLog
            {
                SimaLogTypeId = (int)SimaLogTypeEnum.Information,
                Created = DateTime.Now,
                IpAddress = ipAddress,
                Description = "QrCode uğurla yaradıldı.",
                ErrorMessage = null,
                RequestBody = fullJsonContainer,
                Headers = null
            });


            return RedirectToAction("Index", "Home", new { operationId = operationId });
        }

        [HttpGet]
        public async Task<IActionResult> GetFile(string tsquery)
        {

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var headers = GetHeaders(HttpContext.Request.Headers);


            await _context.LogAsync(new SimaLog
            {
                SimaLogTypeId = (int)SimaLogTypeEnum.Information,
                Created = DateTime.Now,
                IpAddress = ipAddress,
                Description = "GetFile üçün sorğu başladı.",
                ErrorMessage = null,
                RequestBody = tsquery,
                Headers = headers
            });


            #region Header Validation
            //ts cert
            var tcCertResult = HttpContext.Request.Headers.TryGetValue("ts-cert", out StringValues tcCert);
            if (!tcCertResult)
            {
                await _context.LogAsync(new SimaLog
                {
                    SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                    Created = DateTime.Now,
                    IpAddress = ipAddress,
                    Description = "Header xətası baş verdi.",
                    ErrorMessage = "Sorğu zamanı headerda TS - CERT qeyd edilməyib",
                    RequestBody = tsquery,
                    Headers = headers
                });

                return Json(new
                {
                    errormessage = "Header does not have ts-cert"
                });
            }

            //ts-sign
            var tcSignResult = HttpContext.Request.Headers.TryGetValue("ts-sign", out StringValues tcSign);

            if (!tcSignResult)
            {
                await _context.LogAsync(new SimaLog
                {
                    SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                    Created = DateTime.Now,
                    IpAddress = ipAddress,
                    Description = "Header xətası baş verdi.",
                    ErrorMessage = "Sorğu zamanı headerda TS-SIGN qeyd edilməyib",
                    RequestBody = tsquery,
                    Headers = headers
                });

                return Json(new
                {
                    errormessage = "Header does not have ts-sign"
                });
            }


            //ts sign alg
            var tcSignAlgResult = HttpContext.Request.Headers.TryGetValue("ts-sign-alg", out StringValues tcAlg);
            if (!tcSignAlgResult)
            {
                await _context.LogAsync(new SimaLog
                {
                    SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                    Created = DateTime.Now,
                    IpAddress = ipAddress,
                    Description = "Header xətası baş verdi.",
                    ErrorMessage = "Sorğu zamanı headerda TS-SIGN-ALG qeyd edilməyib",
                    RequestBody = tsquery,
                    Headers = headers
                });
                return Json(new
                {
                    errormessage = "Header does not have ts-sign-alg"
                });
            }
            #endregion


            #region CERT VALIDATION
            X509CertificateParser certParser = new X509CertificateParser();

            var cert = certParser.ReadCertificate(Convert.FromBase64String(tcCert));

            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");

            var pub = cert.GetPublicKey();

            var stpem = ConvertToStringPem(pub);

            // Verify using the certificate - the certificate's public key is extracted using the GetPublicKey method.
            signer.Init(false, cert.GetPublicKey());

            //Get query string from url dynamically
            string queryString = HttpContext.Request.GetEncodedPathAndQuery(); // /Home/GetFile/?tsQuery=asdadsa

            var queryStringBuffer = Encoding.UTF8.GetBytes(queryString);
            signer.BlockUpdate(queryStringBuffer, 0, queryStringBuffer.Length);
            var success = signer.VerifySignature(Convert.FromBase64String(tcSign));

            if (!success)
            {

                await _context.LogAsync(new SimaLog
                {
                    SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                    Created = DateTime.Now,
                    IpAddress = ipAddress,
                    Description = "Cert validasiya xətaşı baş verdi.",
                    ErrorMessage = "Cert validasiya edilə bilmədi.",
                    RequestBody = tsquery,
                    Headers = headers
                });

                return Json(new
                {
                    errormessage = "Certificate validation is unsuccessfully"
                });

            }
            #endregion


            try
            {
                byte[] data = Convert.FromBase64String(tsquery);
                string jsonContainer = Encoding.UTF8.GetString(data);

                TSContainer container = JsonConvert.DeserializeObject<TSContainer>(jsonContainer);

                string base64 = String.Empty;
                if (container.SignableContainer.OperationInfo.Type == OperationType.Auth)
                {
                    var challange = Guid.NewGuid().ToString();
                    base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(challange));

                }
                else
                {
                    //You need to find and get correct base64 of file (I will return random file)
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", container.SignableContainer.OperationInfo.OperationId + ".pdf");

                    using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        base64 = Convert.ToBase64String(buffer);
                    }

                }

                return Json(new
                {
                    filename = container.SignableContainer.OperationInfo.Type == OperationType.Auth ? "challange" : container.SignableContainer.OperationInfo.OperationId + ".pdf",
                    data = base64
                });
            }
            catch (Exception exp)
            {
                await _context.LogAsync(new SimaLog
                {
                    SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                    Created = DateTime.Now,
                    IpAddress = ipAddress,
                    Description = "Xəta baş verdi.",
                    ErrorMessage = exp.Message,
                    RequestBody = tsquery,
                    Headers = headers
                });

                return Json(new
                {
                    errormessage = exp.Message
                });
            }
        }
        public static string ConvertToStringPem(AsymmetricKeyParameter pem)
        {
            byte[] publicKeyDer = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pem).GetDerEncoded();
            return Convert.ToBase64String(publicKeyDer);
        }


        [HttpGet]
        public async Task<IActionResult> GetStatus(string operationId)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();



            _logger.Log(LogLevel.Information, $"-------GET STATUS started with IP:{ipAddress}-------");
            var headers = GetHeaders(HttpContext.Request.Headers);
            _logger.Log(LogLevel.Information, $"-------HEADERS :" + headers);
            CallbackPostModel callbackM = null;
            bool result = ApplicationData.PostedOperations.TryGetValue(operationId, out callbackM);

            if (!result)
                return Json(new
                {
                    status = false,
                    callback = ""
                });

            if (callbackM != null)
            {
                return Json(new
                {
                    status = true,
                    callback = callbackM
                });
            }

            return Json(new
            {
                status = false,
                callback = ""
            });
        }


        [HttpPost]
        public async Task<IActionResult> Callback()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var headers = GetHeaders(HttpContext.Request.Headers);


            string body = "";
            CallbackPostModel model = null;

            //get request body
            using (var streamReader = new StreamReader(HttpContext.Request.Body))
            {

                body = await streamReader.ReadToEndAsync();

                await _context.LogAsync(new SimaLog
                {
                    SimaLogTypeId = (int)SimaLogTypeEnum.Information,
                    Created = DateTime.Now,
                    IpAddress = ipAddress,
                    Description = "Cert validasiya xətaşı baş verdi.",
                    ErrorMessage = "Cert validasiya edilə bilmədi.",
                    RequestBody = body,
                    Headers = headers
                });



                model = JsonConvert.DeserializeObject<CallbackPostModel>(body);

              

                //get request body bytes
                var bodyBuffer = Encoding.UTF8.GetBytes(body);

                #region Header Validation
                //ts cert
                var tcCertResult = HttpContext.Request.Headers.TryGetValue("ts-cert", out StringValues tcCert);
                if (!tcCertResult)
                {
                    await _context.LogAsync(new SimaLog
                    {
                        SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                        Created = DateTime.Now,
                        IpAddress = ipAddress,
                        Description = "Header xətası baş verdi.",
                        ErrorMessage = "Sorğu zamanı headerda TS - CERT qeyd edilməyib",
                        RequestBody = body,
                        Headers = headers
                    });

                    return Json(new
                    {
                        errormessage = "Header does not have ts-cert"
                    });
                }

                //ts-sign
                var tcSignResult = HttpContext.Request.Headers.TryGetValue("ts-sign", out StringValues tcSign);

                if (!tcSignResult)
                {
                    await _context.LogAsync(new SimaLog
                    {
                        SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                        Created = DateTime.Now,
                        IpAddress = ipAddress,
                        Description = "Header xətası baş verdi.",
                        ErrorMessage = "Sorğu zamanı headerda TS-SIGN qeyd edilməyib",
                        RequestBody = body,
                        Headers = headers
                    });

                    return Json(new
                    {
                        errormessage = "Header does not have ts-sign"
                    });
                }


                //ts sign alg
                var tcSignAlgResult = HttpContext.Request.Headers.TryGetValue("ts-sign-alg", out StringValues tcAlg);
                if (!tcSignAlgResult)
                {
                    await _context.LogAsync(new SimaLog
                    {
                        SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                        Created = DateTime.Now,
                        IpAddress = ipAddress,
                        Description = "Header xətası baş verdi.",
                        ErrorMessage = "Sorğu zamanı headerda TS-SIGN-ALG qeyd edilməyib",
                        RequestBody = body,
                        Headers = headers
                    });
                    return Json(new
                    {
                        errormessage = "Header does not have ts-sign-alg"
                    });
                }
                #endregion

                //#region CERT VALIDATION
                //X509CertificateParser certParser = new X509CertificateParser();

                //var cert = certParser.ReadCertificate(Convert.FromBase64String(tcCert));

                //var signer = SignerUtilities.GetSigner("SHA-256withECDSA");

                ////var pub = cert.GetPublicKey();

                ////var stpem = ConvertToStringPem(pub);

                //// Verify using the certificate - the certificate's public key is extracted using the GetPublicKey method.
                //signer.Init(false, cert.GetPublicKey());

                //signer.BlockUpdate(bodyBuffer, 0, bodyBuffer.Length);
                //var success = signer.VerifySignature(Convert.FromBase64String(tcSign));

                //if (!success)
                //{
                //    await _context.LogAsync(new SimaLog
                //    {
                //        SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                //        Created = DateTime.Now,
                //        IpAddress = ipAddress,
                //        Description = "Cert validasiya xətaşı baş verdi.",
                //        ErrorMessage = "Cert validasiya edilə bilmədi.",
                //        RequestBody = body,
                //        Headers = headers
                //    });

                //    return Json(new
                //    {
                //        errormessage = "Certificate validation is unsuccessfully"
                //    });
                //}
                //#endregion

            }


            #region Model Validation and OperationId Update
            try
            {

                if (model != null && model.OperationId != null)
                {
                    CallbackPostModel callbackM = null;
                    bool result = ApplicationData.PostedOperations.TryGetValue(model.OperationId, out callbackM);
                    if (result && callbackM == null)
                    {

                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", model.OperationId + ".pdf"));

                        using (FileStream stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", model.OperationId + ".pdf"), FileMode.Create))
                        {
                            var buffer = Convert.FromBase64String(model.DataSignature);
                            stream.Write(buffer, 0, buffer.Length);
                        }
                       
                        ApplicationData.PostedOperations[model.OperationId] = model;
                    }
                }


                return Json(new
                {
                    status = "success"
                });
            }
            catch (Exception exp)
            {
                await _context.LogAsync(new SimaLog
                {
                    SimaLogTypeId = (int)SimaLogTypeEnum.Error,
                    Created = DateTime.Now,
                    IpAddress = ipAddress,
                    Description = "Xəta baş verdi.",
                    ErrorMessage = exp.Message,
                    RequestBody = body,
                    Headers = headers
                });
                return Json(new
                {
                    status = exp.Message
                });

            }
            #endregion

        }

        [HttpGet]
        public IActionResult Clear(string operationId)
        {
            ApplicationData.QrCodes.Remove(operationId);
            ApplicationData.PostedOperations.Remove(operationId);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public static long ConvertDatetimeToUnixTimeStamp(DateTime date)
        {
            DateTime originDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date - originDate;
            return (long)Math.Floor(diff.TotalSeconds);
        }

        private string GetHeaders(IHeaderDictionary headersDictionary)
        {
            string headers = String.Empty;
            foreach (var header in headersDictionary)
            {
                headers += header.Key + ":" + header.Value + ",";
            }

            headers = headers.Remove(headers.LastIndexOf(","), 1);
            return headers;
        }
    }
}
