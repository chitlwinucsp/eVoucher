using eVoucher.Data;
using eVoucher.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eVoucher.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EStoreController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        private readonly IDistributedCache distributedCache;
        private readonly EvoucherDBContext _context;

        private const string _regexVisa = "^4[0-9]{12}(?:[0-9]{3})?$";
        private const string _regexMaster = "^5[1-5][0-9]{14}|^(222[1-9]|22[3-9]\\d|2[3-6]\\d{2}|27[0-1]\\d|2720)[0-9]{12}$";

        public EStoreController(JwtSettings jwtSettings, IDistributedCache distributedCache, EvoucherDBContext evoucherDBContext)
        {
            this.jwtSettings = jwtSettings;
            this.distributedCache = distributedCache;
            this._context = evoucherDBContext;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResPaymethodMethodList> GetPaymentMethodList()
        {
            ResPaymethodMethodList paymethodMethodList = new ResPaymethodMethodList();

            try
            {
                string cacheKey = "paymentMethodLists";

                string serializedPaymentMethodLists;

                var encodedPaymntList = await distributedCache.GetAsync(cacheKey);

                if (encodedPaymntList != null)
                {
                    serializedPaymentMethodLists = Encoding.UTF8.GetString(encodedPaymntList);
                    paymethodMethodList = JsonConvert.DeserializeObject<ResPaymethodMethodList>(serializedPaymentMethodLists);
                }
                else
                {
                    var paymentMethods = _context.Payment_Method.ToList();

                    paymethodMethodList.ResultInfo = new ResultInfo() { Result = true, ErrorInfo = new ErrorInfo() };
                    paymethodMethodList.Payment_Methods = paymentMethods;

                    serializedPaymentMethodLists = JsonConvert.SerializeObject(paymethodMethodList);
                    encodedPaymntList = Encoding.UTF8.GetBytes(serializedPaymentMethodLists);
                    var options = new DistributedCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                                    .SetAbsoluteExpiration(DateTime.Now.AddHours(6));
                    await distributedCache.SetAsync(cacheKey, encodedPaymntList, options);
                }
            }
            catch (Exception ex)
            {
                paymethodMethodList.ResultInfo = new ResultInfo()
                {
                    Result = false,
                    ErrorInfo = new ErrorInfo()
                    {
                        ErrNo = 11,
                        ErrMessage = ex.Message
                    }
                };
                paymethodMethodList.Payment_Methods = null;
            }

            return paymethodMethodList;

        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public ResultInfo MakePaymentWithCreditCard(CreditCard creditCard)
        {
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                if (creditCard == null)
                {
                    resultInfo.Result = false;
                    resultInfo.ErrorInfo = new ErrorInfo() { ErrMessage = "Invalid card number", ErrNo = 11 };
                }
                else
                {
                    switch (creditCard.IssuingNetwork)
                    {
                        case "Visa":
                            resultInfo.Result = new Regex(_regexVisa).IsMatch(creditCard.CardNumber);
                            break;

                        case "MasterCard":
                            resultInfo.Result = new Regex(_regexMaster).IsMatch(creditCard.CardNumber);
                            break;
                        default:
                            throw new Exception("Supported cards (Visa, MasterCard).Other cards does not support for now.");
                    }
                }
            }
            catch (Exception ex)
            {
                resultInfo.Result = false;
                resultInfo.ErrorInfo = new ErrorInfo() { ErrNo = 11, ErrMessage = ex.Message };
            }

            return resultInfo;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResEvoucherList> GetEVoucherList()
        {
            ResEvoucherList resEvoucherList = new ResEvoucherList();

            try
            {
                List<string> voucherList;

                string cacheKey = "getVoucherLists";

                string serializedVoucherLists;

                var encodedVoucherList = await distributedCache.GetAsync(cacheKey);

                if (encodedVoucherList != null)
                {
                    serializedVoucherLists = Encoding.UTF8.GetString(encodedVoucherList);
                    voucherList = JsonConvert.DeserializeObject<List<string>>(serializedVoucherLists);
                }
                else
                {
                    voucherList = _context.Voucher.Select(x => x.name).ToList();

                    serializedVoucherLists = JsonConvert.SerializeObject(voucherList);
                    encodedVoucherList = Encoding.UTF8.GetBytes(serializedVoucherLists);
                    var options = new DistributedCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                                    .SetAbsoluteExpiration(DateTime.Now.AddHours(1));
                    await distributedCache.SetAsync(cacheKey, encodedVoucherList, options);
                }

                resEvoucherList = new ResEvoucherList()
                {
                    ResultInfo = new ResultInfo() { Result = true },
                    VoucherList = voucherList
                };
            }
            catch (Exception ex)
            {
                resEvoucherList = new ResEvoucherList()
                {
                    ResultInfo = new ResultInfo()
                    {
                        Result = false,
                        ErrorInfo = new ErrorInfo()
                        {
                            ErrMessage = ex.Message,
                            ErrNo = 11 // can customize the error no.
                        }
                    }
                };
            }

            return resEvoucherList;
        }


        [HttpGet]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResEvoucherDetail> GetEVoucherDetail(int eVoucherId, string eVoucherName)
        {
            ResEvoucherDetail resEvoucherDetail = new ResEvoucherDetail();

            try
            {
                voucher eVoucherDetail;

                string cacheKey = string.Format("eVoucher_Detail_{0}_{1}", eVoucherId, eVoucherName);

                string serializedVoucherDetail;

                var encodedVoucherDetail = await distributedCache.GetAsync(cacheKey);

                if (encodedVoucherDetail != null)
                {
                    serializedVoucherDetail = Encoding.UTF8.GetString(encodedVoucherDetail);
                    eVoucherDetail = JsonConvert.DeserializeObject<voucher>(serializedVoucherDetail);
                }
                else
                {
                    eVoucherDetail = _context.Voucher.FirstOrDefault(x => x.id == eVoucherId || x.name == eVoucherName);

                    if (eVoucherDetail == null)
                    {
                        throw new Exception(string.Format("Evoucher does not exist. Evoucher Id : {0}", eVoucherId));
                    }

                    serializedVoucherDetail = JsonConvert.SerializeObject(eVoucherDetail);
                    encodedVoucherDetail = Encoding.UTF8.GetBytes(serializedVoucherDetail);
                    var options = new DistributedCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                                    .SetAbsoluteExpiration(DateTime.Now.AddHours(1));
                    await distributedCache.SetAsync(cacheKey, encodedVoucherDetail, options);
                }

                resEvoucherDetail = new ResEvoucherDetail()
                {
                    ResultInfo = new ResultInfo() { Result = true },
                     EvoucherDetail = eVoucherDetail
                };

            }
            catch (Exception ex)
            {
                resEvoucherDetail = new ResEvoucherDetail()
                {
                    ResultInfo = new ResultInfo()
                    {
                        Result = false,
                        ErrorInfo = new ErrorInfo()
                        {
                            ErrMessage = ex.Message,
                            ErrNo = 11 // can customize the error no.
                        }
                    }
                };
            }

            return resEvoucherDetail;
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public ResVerifyPromoCode VerifyPromoCode(int eVoucherId)
        {
            ResVerifyPromoCode resVerifyPromoCode = new ResVerifyPromoCode();

            try
            {
                bool eVoucherStatus;

                var eVoucher = _context.Voucher.FirstOrDefault(x => x.id == eVoucherId);

                if (eVoucher == null)
                {
                    throw new Exception(string.Format("Evoucher does not exist. Evoucher Id : {0}", eVoucherId));
                }

                eVoucherStatus = eVoucher.active;

                resVerifyPromoCode = new ResVerifyPromoCode()
                {
                    ResultInfo = new ResultInfo() { Result = true },
                    EVoucherStatus = eVoucherStatus
                };

            }
            catch (Exception ex)
            {
                resVerifyPromoCode = new ResVerifyPromoCode()
                {
                    ResultInfo = new ResultInfo()
                    {
                        Result = false,
                        ErrorInfo = new ErrorInfo()
                        {
                            ErrMessage = ex.Message,
                            ErrNo = 11 // can customize the error no.
                        }
                    }
                };
            }

            return resVerifyPromoCode;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResCheckOut> CheckOut(ReqCheckOut checkOutData)
        {
            ResCheckOut resCheckOut = new ResCheckOut();

            try
            {
                if (checkOutData == null)
                {
                    throw new Exception("Invalid data of CheckOut.");
                }

                if (string.IsNullOrEmpty(checkOutData.Phone_no))
                {
                    throw new Exception("Checkout phone no does not allow null.");
                }

                voucher voucherData = _context.Voucher.FirstOrDefault(x => x.id == checkOutData.EVoucherId);

                if (voucherData == null)
                {
                    throw new Exception(string.Format("This eVoucher does not exist. Id:{0}", checkOutData.EVoucherId));
                }

                if (!string.Equals(checkOutData.Phone_no, voucherData.phone_no))
                {
                    throw new Exception(string.Format("Phone no does not match. (EVoucher Ph No:{0}, CheckOut Ph No:{1})",
                        voucherData.phone_no, checkOutData.Phone_no));
                }

                if (voucherData.qty < checkOutData.Qty)
                {
                    throw new Exception(string.Format("Not enough qty. Stock qty:{0}, Checkout qty:{1}",
                        voucherData.qty, checkOutData.Qty));
                }

                decimal cost = voucherData.amount * checkOutData.Qty;
                decimal discount = voucherData.discount * cost;
                decimal total_cost = cost - discount;

                Purchase_History purchase_History = new Purchase_History()
                {
                    voucher_id = voucherData.id,
                    phone_no = voucherData.phone_no,
                    qty = checkOutData.Qty,
                    cost = cost,
                    discount = discount,
                    total_cost = total_cost,
                    used = false
                };

                voucherData.qty = voucherData.qty - checkOutData.Qty;
                voucherData.updated_date = DateTime.Now;

                resCheckOut.EVoucherId = voucherData.id;
                resCheckOut.EvoucherTitle = voucherData.name;
                resCheckOut.Discount = discount;
                resCheckOut.Cost = cost;
                resCheckOut.Total_Cost = total_cost;
                resCheckOut.Qty = checkOutData.Qty;

                _context.Purchase_History.Add(purchase_History);

                await _context.SaveChangesAsync();

                resCheckOut.TransactionId = purchase_History.id;
            }
            catch (Exception ex)
            {
                resCheckOut = new ResCheckOut()
                {
                    ResultInfo = new ResultInfo()
                    {
                        Result = false,
                        ErrorInfo = new ErrorInfo()
                        {
                            ErrNo = 11,
                            ErrMessage = ex.Message
                        }
                    }
                };

            }

            return resCheckOut;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResultInfo> SetUsed(ReqSetUsed reqSetUsed)
        {
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                if (reqSetUsed == null)
                {
                    throw new Exception("Unable to update the status. Invaild data.");
                }

                Purchase_History history = _context.Purchase_History.FirstOrDefault(x => x.id == reqSetUsed.TransactionId);
                if (history == null)
                {
                    throw new Exception("There is no data to update.");
                }

                history.used = reqSetUsed.Status;

                await _context.SaveChangesAsync();

                resultInfo.Result = true;
            }
            catch (Exception ex)
            {
                resultInfo.Result = false;
                resultInfo.ErrorInfo = new ErrorInfo() { ErrMessage = ex.Message, ErrNo = 11 };
            }

            return resultInfo;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResPurchaseHistory> GetPurchaseHistory(ReqEvoucherStatus evoucherStatus)
        {
            ResPurchaseHistory resPurchaseHistory = new ResPurchaseHistory();

            try
            {
                List<Purchase_History> lstpurchaseHist;

                if (evoucherStatus == null)
                {
                    throw new Exception("Invalid data.");
                }
                
                string cacheKey = string.Format("purchaseHistory_{0}", evoucherStatus.Status);

                string serializedPurchaseHist;

                var encodedPurchaseHist = await distributedCache.GetAsync(cacheKey);

                if (encodedPurchaseHist != null)
                {
                    serializedPurchaseHist = Encoding.UTF8.GetString(encodedPurchaseHist);
                    lstpurchaseHist = JsonConvert.DeserializeObject<List<Purchase_History>>(serializedPurchaseHist);
                }
                else
                {
                    lstpurchaseHist = _context.Purchase_History.Where(x => x.used == evoucherStatus.Status || x.id == evoucherStatus.EvoucherId).ToList();

                    if (lstpurchaseHist == null)
                    {
                        throw new Exception(string.Format("There is no transaction. Status:{0}", evoucherStatus.Status));
                    }

                    serializedPurchaseHist = JsonConvert.SerializeObject(lstpurchaseHist);
                    encodedPurchaseHist = Encoding.UTF8.GetBytes(serializedPurchaseHist);
                    var options = new DistributedCacheEntryOptions()
                                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                                    .SetAbsoluteExpiration(DateTime.Now.AddHours(1));
                    await distributedCache.SetAsync(cacheKey, encodedPurchaseHist, options);
                }

                resPurchaseHistory = new ResPurchaseHistory()
                {
                    ResultInfo = new ResultInfo() { Result = true },
                    Purchase_Histories = lstpurchaseHist
                };

            }
            catch (Exception ex)
            {
                resPurchaseHistory = new ResPurchaseHistory()
                {
                    ResultInfo = new ResultInfo()
                    {
                        Result = false,
                        ErrorInfo = new ErrorInfo()
                        {
                            ErrMessage = ex.Message,
                            ErrNo = 11
                        }
                    }
                };
            }

            return resPurchaseHistory;
        }
    }
}
