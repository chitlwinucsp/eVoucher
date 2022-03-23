using eVoucher.Data;
using eVoucher.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EVoucherController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        private readonly IDistributedCache distributedCache;
        private readonly EvoucherDBContext _context;

        public EVoucherController(JwtSettings jwtSettings, IDistributedCache distributedCache, EvoucherDBContext evoucherDBContext)
        {
            this.jwtSettings = jwtSettings;
            this.distributedCache = distributedCache;
            this._context = evoucherDBContext;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResCreateEvoucher> CreateEvoucher(ReqCreateEvoucher reqVoucher)
        {
            ResCreateEvoucher resCreateEvoucher = new ResCreateEvoucher();

            try
            {
                // try add validation 
                if (reqVoucher == null)
                {
                    throw new ArgumentNullException();
                }

                if (string.IsNullOrEmpty(reqVoucher.title))
                {
                    throw new Exception("Voucher name is null or empy");
                }

                if (string.IsNullOrEmpty(reqVoucher.expiry_date))
                {
                    throw new Exception("Expiry date is null or empty");
                }

                if (reqVoucher.BuyMyself == null && reqVoucher.GiftToOthers == null)
                {
                    throw new Exception("Buy type information is null");
                }

                if ((reqVoucher.buy_type == "1" || reqVoucher.buy_type == "only_me_usage") && reqVoucher.BuyMyself == null)
                {
                    throw new Exception("Invalid data of BuyMySelf fields.");
                }

                if ((reqVoucher.buy_type == "2" || reqVoucher.buy_type == "gift_to_others") && reqVoucher.GiftToOthers == null)
                {
                    throw new Exception("Invalid data of GiftToOthers fields.");
                }

                voucher data = new voucher();

                DateTime expDate;

                if (DateTime.TryParse(reqVoucher.expiry_date, out expDate))
                {
                    data.expiry_date = expDate;
                }
                else
                {
                    throw new Exception("Invalid expiry date. use dd/mm/yyyy");
                }

                switch (reqVoucher.buy_type)
                {
                    case "1":
                    case "only_me_usage":
                        if (string.IsNullOrEmpty(reqVoucher.BuyMyself.name))
                        {
                            throw new Exception("Invalid user name.");
                        }
                        
                        if (string.IsNullOrEmpty(reqVoucher.BuyMyself.phone_no))
                        {
                            throw new Exception("Invalid phone no.");
                        }

                        data.phone_no = reqVoucher.BuyMyself.phone_no;
                        data.user_name = reqVoucher.BuyMyself.name;
                        data.max_limit = reqVoucher.BuyMyself.max_limit;
                        data.buy_type = 1;

                        break;
                    case "2":
                    case "gift_to_others":
                        if (string.IsNullOrEmpty(reqVoucher.GiftToOthers.name))
                        {
                            throw new Exception("Invalid user name.");
                        }

                        if (string.IsNullOrEmpty(reqVoucher.GiftToOthers.phone_no))
                        {
                            throw new Exception("Invalid phone no.");
                        }

                        data.user_name = reqVoucher.GiftToOthers.name;
                        data.phone_no = reqVoucher.GiftToOthers.phone_no;
                        data.min_user_limit = reqVoucher.GiftToOthers.git_per_user_limit;
                        data.max_limit = reqVoucher.GiftToOthers.max_limit;
                        data.buy_type = 2;

                        break;
                    default:
                        throw new Exception("Invalid buy type");
                }

                data.name = reqVoucher.title;
                data.payment_method = reqVoucher.payment_method;
                data.qty = reqVoucher.qty;
                data.discount = reqVoucher.discount;
                data.description = reqVoucher.description;
                data.image = reqVoucher.image;
                data.amount = reqVoucher.amount;
                data.discount = reqVoucher.discount;
                data.active = reqVoucher.active;

                _context.Voucher.Add(data);
                await _context.SaveChangesAsync();

                resCreateEvoucher.id = data.id;
                resCreateEvoucher.ResultInfo = new ResultInfo() { Result = true };
            }
            catch (Exception ex)
            {
                resCreateEvoucher.ResultInfo = new ResultInfo()
                {
                    ErrorInfo = new ErrorInfo()
                    {
                        ErrMessage = ex.Message,
                        ErrNo = 11
                    },
                    Result = false
                };
            }

            return resCreateEvoucher;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResultInfo> UpdateEvoucher(int id, ReqCreateEvoucher reqEvoucher)
        {
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                if (reqEvoucher == null)
                {
                    throw new Exception("Update data is null or empty.");
                }

                var data = _context.Voucher.FirstOrDefault(x => x.id == id);
                if (data == null)
                {
                    throw new Exception(string.Format("Unable to update data. id:{0}", id));
                }

                data.active = reqEvoucher.active;
                data.amount = reqEvoucher.amount;
                data.discount = reqEvoucher.discount;
                data.qty = reqEvoucher.qty;

                if (string.IsNullOrEmpty(reqEvoucher.title) == false)
                {
                    data.name = reqEvoucher.title;
                }

                if (string.IsNullOrEmpty(reqEvoucher.description) == false)
                {
                    data.description = reqEvoucher.description;
                }

                DateTime expDate;

                if ((string.IsNullOrEmpty(reqEvoucher.expiry_date) == false) &&
                    DateTime.TryParse(reqEvoucher.expiry_date, out expDate))
                {
                    data.expiry_date = expDate;
                }

                data.payment_method = reqEvoucher.payment_method;

                if (string.IsNullOrEmpty(reqEvoucher.buy_type) == false)
                {

                    if ((reqEvoucher.buy_type == "1" || reqEvoucher.buy_type == "only_me_usage") && reqEvoucher.BuyMyself == null)
                    {
                        throw new Exception("Invalid data of BuyMySelf fields.");
                    }

                    if ((reqEvoucher.buy_type == "2" || reqEvoucher.buy_type == "gift_to_others") && reqEvoucher.GiftToOthers == null)
                    {
                        throw new Exception("Invalid data of GiftToOthers fields.");
                    }

                    switch (reqEvoucher.buy_type)
                    {
                        case "1":
                        case "only_me_usage":
                            if (string.IsNullOrEmpty(reqEvoucher.BuyMyself.name) == false)
                            {
                                data.user_name = reqEvoucher.BuyMyself.name;
                            }

                            if (string.IsNullOrEmpty(reqEvoucher.BuyMyself.phone_no) == false)
                            {
                                data.phone_no = reqEvoucher.BuyMyself.phone_no;
                            }

                            data.max_limit = reqEvoucher.BuyMyself.max_limit;
                            data.buy_type = 1;

                            break;
                        case "2":
                        case "gift_to_others":
                            if (string.IsNullOrEmpty(reqEvoucher.GiftToOthers.name))
                            {
                                data.user_name = reqEvoucher.GiftToOthers.name;
                            }

                            if (string.IsNullOrEmpty(reqEvoucher.GiftToOthers.phone_no))
                            {
                                data.phone_no = reqEvoucher.GiftToOthers.phone_no;
                            }
                            
                            data.min_user_limit = reqEvoucher.GiftToOthers.git_per_user_limit;
                            data.max_limit = reqEvoucher.GiftToOthers.max_limit;
                            data.buy_type = 2;

                            break;
                        default:
                            break;
                    }

                    

                }

                data.updated_date = DateTime.Now;

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

        [HttpPost]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ResultInfo> SetEvoucherStatus(ReqEvoucherStatus evoucherStatus)
        {
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                if (evoucherStatus == null)
                {
                    throw new Exception("Invalid data.");
                }

                voucher voucherData = _context.Voucher.FirstOrDefault(x => x.id == evoucherStatus.EvoucherId);
                if (voucherData == null)
                {
                    throw new Exception(string.Format("The evoucher does not exist. Id:{0}", evoucherStatus.EvoucherId));
                }

                voucherData.active = evoucherStatus.Status;

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
    }
}
