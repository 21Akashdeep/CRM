using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoGrn
    {
        private readonly DBCRM db;
        private readonly AppSetting App = Util.AppSetting;
        public RepoGrn(DBCRM db)
        {
            this.db = db;
           
        }
        public async Task<Message> GetViewOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                Option.Status = await (
                    from ac in db.Voucher
                    join st in db.Setting on new { Value = ac.Status.ToString(), Name = App.SettingName.Status } equals new { st.Value, st.Name }
                    group st by new { st.Value, st.Description } into st
                    select new
                    {
                        st.Key.Value,
                        st.Key.Description
                    }
                ).ToListAsync();
                Option.Customer = await db.Customer.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                }).ToListAsync();
                Option.Location = await db.Location.Where(ag => App.ActiveStatus.Contains(ag.Status)).Select(ag => new
                {
                    ag.Id,
                    ag.Code,
                    ag.Name,
                    ag.Description,
                }).ToListAsync();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> GetAddOptionAsync()
        {
            Message objMsg = new Message();
            try
            {
                //var Party = await db.Customer.Where(par => App.ActiveStatus.Contains(par.Status) && par.CustomerType == App.AccountType.Purchase).Select(par => new
                //{
                //    par.Id,
                //    par.Code,
                //    par.Name,
                //    par.Description
                //}).ToListAsync();
                var Customer = await db.Customer.Where(shf => App.ActiveStatus.Contains(shf.Status)).Select(shf => new
                {
                    shf.Id,
                    shf.Code,
                    shf.Name,
                    shf.Description
                }).ToListAsync();
                var Item = await (
                    from itm in db.Item
                    join unt in db.Unit on itm.UnitId equals unt.Id
                    //join prl in db.PriceList on itm.Id equals prl.ItemId
                    select new
                    {
                        itm.Id,
                        itm.Code,
                        itm.Name,
                        itm.Description,
                        itm.UnitId,
                        UnitDesc = unt.Description,
                        //prl.Rate
                    }
                ).ToListAsync();
                objMsg.data = new
                {
                    //Party,
                    Customer,
                    Item,
                };
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<Voucher>> ListAsync(Voucher? obj, User User)
        {
            obj ??= new Voucher();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbGrnQuery = db.Voucher.Where(grn => obj.ListStatus.Contains(grn.Status)).AsQueryable();
            //Add Filter
            if (obj.ListId.Any())
                dbGrnQuery = dbGrnQuery.Where(grn => obj.ListId.Contains(grn.Id));
            if (obj.FromDate != DateTime.MinValue)
                dbGrnQuery = dbGrnQuery.Where(grn => grn.Date.Date >= obj.FromDate.Date);
            if (obj.ToDate != DateTime.MinValue)
                dbGrnQuery = dbGrnQuery.Where(grn => grn.Date.Date <= obj.ToDate.Date);
            //if (obj.ListPartyId.Any())
            //    dbGrnQuery = dbGrnQuery.Where(grn => obj.ListPartyId.Contains(grn.CustomerId));
            //if (obj.ListShiftId.Any())
            //    dbGrnQuery = dbGrnQuery.Where(grn => obj.ListShiftId.Contains(grn.ShiftId));

            var dbGrn = await (
                from grn in dbGrnQuery
                join par in db.Customer on grn.CustomerId equals par.Id
                join shi in db.Customer on grn.CustomerId equals shi.Id
                //join sts in db.Setting on new { Category = App.Category.Status, Value = grn.Status.ToString() } equals new { sts.Category, sts.Value }
                join cby in db.User on grn.CreatedBy equals cby.Id
                join uby in db.User on grn.UpdatedBy equals uby.Id
                select new
                {
                    Grn = grn,
                    PartyDesc = par.Description,
                    ShiftDesc = shi.Description,
                    CoName =grn.ConName,    
                    //StatusDesc = sts.Description,
                    //StatusCss = sts.CssClass,
                    CreatedByName = cby.Name,
                    UpdatedByName = uby.Name
                }
            ).ToListAsync();

            var dbGrnItem = await (
                from gri in db.Item
                join cby in db.User on gri.CreatedBy equals cby.Id
                join uby in db.User on gri.UpdatedBy equals uby.Id
                //where gri.Status == grn.Status
                select new Item
                {
                    //Id = gri.Id,
                    Name = gri.Name,
                    //ItemId = gri.ItemId,
                    //ItemDesc = itm.Description,
                    //UnitDesc = unt.Description,
                    //Qty = gri.Qty,
                    //Rate = gri.Rate,
                    //Amount = gri.Amount,
                    //Remarks = gri.Remarks,
                    Status = gri.Status,
                    //StatusDesc = sts.Description,
                    //StatusCss = sts.CssClass,
                    CreatedBy = gri.CreatedBy,
                    CreatedByName = cby.Name,
                    CreatedAt = gri.CreatedAt,
                    UpdatedBy = gri.UpdatedBy,
                    UpdatedByName = uby.Name,
                    UpdatedAt = uby.UpdatedAt
                }
            ).ToListAsync();
            //var GrnItemLookup = dbGrnItem.ToLookup(x => x.GrnId);
            var Grn = dbGrn.Select(x =>
            {
                var grn = x.Grn;
                return new Voucher
                {
                    Id = grn.Id,
                    //GrnNo = grn.GrnNo,
                    Date = grn.Date,
                    //PartyId = grn.PartyId,
                    //PartyDesc = x.PartyDesc,
                    //ShiftId = grn.ShiftId,
                    //ShiftDesc = x.ShiftDesc,
                    //VehicleNo = grn.VehicleNo,
                    Remarks = grn.Remarks,
                    //NetAmount = grn.NetAmount,
                    Status = grn.Status,
                    //StatusDesc = x.StatusDesc,
                    //StatusCss = x.StatusCss,
                    CreatedBy = grn.CreatedBy,
                    CreatedByName = x.CreatedByName,
                    CreatedAt = grn.CreatedAt,
                    UpdatedBy = grn.UpdatedBy,
                    UpdatedByName = x.UpdatedByName,
                    UpdatedAt = grn.UpdatedAt,
                    //GrnItem = GrnItemLookup[grn.Id].ToList(),
                    IsEdit = true,
                    IsPrint = true,
                    IsDelete = grn.Status == App.Status.Enable ? true : false,
                };
            }
            ).ToList();
            return Grn;
        }
        public async Task<Message> GetAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await ListAsync(obj, User);
                Message.Get(ref objMsg);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> PrintAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await ListAsync(obj, User);
                Message.Get(ref objMsg);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> ExportAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbGrn = await ListAsync(obj, User);

                var Grn = dbGrn.Select((x, index) => new
                {
                    SlNo = index + 1,
                    x.No,
                    x.Date,
                    //x.PartyDes,
                    //x.ShiftDesc,
                    //x.VehicleNo,
                    x.Remarks,
                    //x.NetAmount,
                    x.StatusDesc,
                    x.CreatedByName,
                    x.CreatedAt,
                    x.UpdatedByName,
                    x.UpdatedAt,
                }).ToList();

                //var GrnItem = dbGrn.SelectMany(gr => gr.Grn.Select(gi => new
                //{
                //    gr.No,
                //    gr.Date,
                //    //gr.PartyDesc,
                //    //gr.ShiftDesc,
                //    //gr.VehicleNo,
                //    GrnRemarks = gr.Remarks,
                //    gi.ItemDesc,
                //    gi.Qty,
                //    gi.Rate,
                //    gi.Amount,
                //    gi.Remarks,
                //    gi.StatusDesc,
                //    gi.CreatedByName,
                //    gi.CreatedAt,
                //    gi.UpdatedByName,
                //    gi.UpdatedAt
                //})).ToList();
                var Company = db.Company.FirstOrDefault() ?? new Company();
                //Convert Excel and then base64
                //using (var workbook = new XLWorkbook())
                //{
                //    Util.AddSheet(workbook, Grn, "Grn", "GRN Report", Company.Description);
                //    Util.AddSheet(workbook, GrnItem, "GrnItem", "GRN Item Report", Company.Description);
                //    using (var stream = new MemoryStream())
                //    {
                //        workbook.SaveAs(stream);
                //        var bytes = stream.ToArray();
                //        var base64 = Convert.ToBase64String(bytes);
                //        objMsg.base64 = base64;
                //    }
                //}
                if (!String.IsNullOrEmpty(objMsg.base64))
                {
                    Message.Success(ref objMsg, "Record found");
                }
                else
                {
                    Message.Error(ref objMsg, "Record did not find.");
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> AddAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var strategy = db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    //await using var transaction = await db.Database.BeginTransactionAsync();
                    try
                    {
                        obj.CreatedBy = User.Id;
                        obj.CreatedAt = DateTime.Now;
                        obj.UpdatedBy = User.Id;
                        obj.UpdatedAt = DateTime.Now;
                        obj.Item.ForEach(gi =>
                        {
                            gi.CreatedBy = User.Id;
                            gi.CreatedAt = DateTime.Now;
                            gi.UpdatedBy = User.Id;
                            gi.UpdatedAt = DateTime.Now;
                        });
                        db.Add(obj);
                        int isSaved = await db.SaveChangesAsync();
                        if (isSaved <= 0)
                        {
                            Message.Error(ref objMsg, "Grn was not saved. The entire transaction has been rolled back.");
                            //await transaction.RollbackAsync();
                            return;
                        }
                        db.Entry(obj).Reload();
                        //var srockTran = obj.Item.Select(gi => new StockTran
                        //{
                        //    Date = obj.Date,
                        //    RefType = App.RefType.Grn,
                        //    RefId = obj.Id,
                        //    RefCode = obj.GrnNo,
                        //    RefItemId = gi.Id,
                        //    ItemId = gi.ItemId,
                        //    ReceivedQty = gi.Qty,
                        //    ConsumedQty = 0,
                        //    Status = App.Status.Enable
                        //}).ToList();
                        //var msg = await RepoStockTran.UpdateAsync(srockTran, User);
                        //isSaved = msg.status == Message.Type.error ? 0 : 1;
                        //if (msg.status == Message.Type.error)
                        //{
                        //    Message.Error(ref objMsg, "Stock transaction was not saved. The entire transaction has been rolled back.");
                        //    await transaction.RollbackAsync();
                        //    return;
                        //}
                        //await transaction.CommitAsync();
                        //Message.Add(ref objMsg, isSaved);
                    }
                    catch (Exception ex)
                    {
                        Message.Exception(ref objMsg, ex);
                        //await transaction.RollbackAsync();
                    }
                });
                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = (await ListAsync(new Voucher
                    {
                        ListId = new List<int> { obj.Id }
                    }, User)).FirstOrDefault();
                    objMsg.data = new
                    {
                        ViewOption = (await GetViewOptionAsync()).data,
                        AddOption = (await GetAddOptionAsync()).data,
                    };
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> EditAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.obj = (await ListAsync(new Voucher
                {
                    ListId = new List<int> { Id },
                    ListStatus = new List<int>(App.ActiveStatus) { App.Status.Delete }
                }, User)).FirstOrDefault();
                if (objMsg.obj == null)
                {
                    Message.Error(ref objMsg, "Grn was not found for edit.");
                    return objMsg;
                }
                objMsg.data = (await GetAddOptionAsync()).data;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> UpdateAsync(Voucher obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                obj.Item = obj.Item.Where(gi => gi.Id > 0).ToList();
                var Grn = await db.Voucher.Include(grn => grn.Item).FirstOrDefaultAsync(x => App.ActiveStatus.Contains(x.Status) && x.Id == obj.Id);
                if (Grn == null)
                {
                    Message.Error(ref objMsg, "Grn was not found for update");
                    return objMsg;
                }
                var startegy = db.Database.CreateExecutionStrategy();
                await startegy.ExecuteAsync(async () =>
                {
                    await using var transaction = await db.Database.BeginTransactionAsync();
                    try
                    {
                        Grn.Date = obj.Date;
                        //Grn.PartyId = obj.PartyId;
                        //Grn.ShiftId = obj.ShiftId;
                        //Grn.VehicleNo = obj.VehicleNo;
                        Grn.Remarks = obj.Remarks;
                        //Grn.NetAmount = obj.NetAmount;
                        Grn.UpdatedBy = User.Id;
                        Grn.UpdatedAt = DateTime.Now;
                        Grn.Item.ForEach(gi =>
                        {
                            var grnItem = obj.Item.FirstOrDefault(gi1 => gi1.Id == gi.Id);
                            if (grnItem != null)
                            {
                                gi.Id = grnItem.Id;
                                //gi.Qty = grnItem.Qty;
                                //gi.Rate = grnItem.Rate;
                                //gi.Amount = grnItem.Amount;
                                //gi.Remarks = grnItem.Remarks;
                            }
                            else
                            {
                                gi.Status = App.Status.ItemDelete;
                            }
                            gi.UpdatedBy = User.Id;
                            gi.UpdatedAt = DateTime.Now;
                        });
                        db.Update(Grn);
                        //Add New Item
                        var newGrnItem = obj.Item.Where(gi => gi.Id == 0).ToList();
                        newGrnItem.ForEach(gi =>
                        {
                            gi.Id = Grn.Id;
                            gi.Status = Grn.Status;
                            gi.CreatedBy = User.Id;
                            gi.CreatedAt = DateTime.Now;
                            gi.UpdatedBy = User.Id;
                            gi.UpdatedAt = DateTime.Now;
                        });
                        await db.AddRangeAsync(newGrnItem);
                        int isSaved = await db.SaveChangesAsync();
                        if (isSaved <= 0)
                        {
                            Message.Error(ref objMsg, "Grn was not upated. The entire transaction has been rolled back.");
                            return;
                        }
                        //Update Stock tran
                        //var stockTran = Grn.Item.Select(gi => new StockTran
                        //{
                        //    Date = Grn.Date,
                        //    RefType = App.RefType.Grn,
                        //    RefId = Grn.Id,
                        //    RefCode = Grn.GrnNo,
                        //    RefItemId = gi.Id,
                        //    ItemId = gi.ItemId,
                        //    ReceivedQty = gi.Qty,
                        //    Status = gi.Status
                        //}).ToList();

                        //var msg = await RepoStockTran.UpdateAsync(stockTran, User);
                        //isSaved = msg.status == Message.Type.error ? 0 : 1;
                        //if (msg.status == Message.Type.error)
                        //{
                        //    Message.Error(ref objMsg, "Stock transaction was not saved. The entire transaction has been rolled back.");
                        //    await transaction.RollbackAsync();
                        //    return;
                        //}
                        //await transaction.CommitAsync();
                        //Message.Update(ref objMsg, isSaved);
                    }
                    catch (Exception ex)
                    {
                        Message.Exception(ref objMsg, ex);
                        await transaction.RollbackAsync();
                    }
                });
                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = (await ListAsync(new Voucher
                    {
                        ListId = new List<int> { obj.Id }
                    }, User)).FirstOrDefault();
                    objMsg.data = new
                    {
                        ViewOption = (await GetViewOptionAsync()).data,
                    };
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> DeleteAsync(int Id, User User)
        {
            Message objMsg = new Message();
            try
            {
                var Grn = await db.Voucher.Include(grn => grn.Item).FirstOrDefaultAsync(grn => App.ActiveStatus.Contains(grn.Status) && grn.Id == Id);
                if (Grn == null)
                {
                    Message.Error(ref objMsg, "Grn was not found for Delete");
                    return objMsg;
                }
                var startegy = db.Database.CreateExecutionStrategy();
                await startegy.ExecuteAsync(async () =>
                {
                    await using var transaction = await db.Database.BeginTransactionAsync();
                    try
                    {
                        Grn.Status = App.Status.Delete;
                        Grn.UpdatedBy = User.Id;
                        Grn.UpdatedAt = DateTime.Now;
                        Grn.Item.ForEach(gi =>
                        {
                            gi.Status = App.ActiveStatus.Contains(gi.Status) ? App.Status.Delete : gi.Status;
                            gi.UpdatedBy = User.Id;
                            gi.UpdatedAt = DateTime.Now;
                        });
                        db.Update(Grn);
                        int isSaved = await db.SaveChangesAsync();
                        if (isSaved <= 0)
                        {
                            Message.Error(ref objMsg, "Grn was not deleted.The entire transaction has been rolled back.");
                            await transaction.RollbackAsync();
                            return;
                        }

                        //var stockTran = await db.StockTran.Where(st => st.Status == App.Status.Enable && st.RefType == App.RefType.Grn && st.RefId == Grn.Id).ToListAsync();
                        //stockTran.ForEach(st =>
                        //{
                        //    st.Status = App.Status.Delete;
                        //    st.UpdatedBy = User.Id;
                        //    st.UpdatedAt = DateTime.Now;
                        //});
                        //var msg = await RepoStockTran.UpdateAsync(stockTran, User);
                        //isSaved = msg.status == Message.Type.error ? 0 : 1;
                        //if (isSaved <= 0)
                        //{
                        //    Message.Error(ref objMsg, "Stock transaction was not saved. The entire transaction has been rolled back.");
                        //    await transaction.RollbackAsync();
                        //    return;
                        //}
                        //await transaction.CommitAsync();
                        //Message.Delete(ref objMsg, isSaved);
                    }
                    catch (Exception ex)
                    {
                        Message.Exception(ref objMsg, ex);
                        await transaction.RollbackAsync();
                        return;
                    }
                });

                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = (await ListAsync(new Voucher
                    {
                        ListId = new List<int> { Grn.Id },
                        ListStatus = new List<int> { App.Status.Delete }
                    }, User)).FirstOrDefault();
                    objMsg.data = new
                    {
                        ViewOption = (await GetViewOptionAsync()).data,
                        AddOption = (await GetAddOptionAsync()).data,
                    };
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
    }
}
