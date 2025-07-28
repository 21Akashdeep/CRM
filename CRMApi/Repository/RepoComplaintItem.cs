using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;

namespace CRMApi.Repository
{
    public class RepoComplaintItem
    {
        private readonly DBCRM db;
        AppSetting App = Util.AppSetting;
        public RepoComplaintItem(DBCRM _db)
        {
            db = _db;
        }
        public async Task<Message> GetViewOptionAsync() 
        { 
            Message objMsg = new Message();
            try 
            {
                              
                dynamic Option = new ExpandoObject();
                var ListStatus = new List<string>() { App.Status.Enable.ToString(), App.Status.Delete.ToString() };
                Option.Status = await db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && ListStatus.Contains(st.Value)).Select(x => new
                {
                    x.Value,
                    x.Description
                }).ToListAsync();

                Option.Customer = await(
                    from cu in db.Customer
                    join ad in db.AdminDiv on cu.AdminDivId equals ad.Id
                    join co in db.Country on cu.CountryId equals co.Id
                    where App.ActiveStatus.Contains(cu.Status)
                    select new
                    {
                        cu.Id,
                        cu.Code,
                        cu.Name,
                        cu.Description,
                        Address = Util.AddressDesc(new Composite.AddressDesc
                        {
                            Add1 = cu.Address1,
                            Add2 = cu.Address2,
                            PinCode = cu.PinCode,
                            PostOffice = cu.PostOffice,
                            District = cu.District,
                            State = ad.Description,
                            StateCode = ad.Code,
                            Country = co.Description,
                            OtherText = cu.ContactNo
                        })
                    }
                ).ToListAsync();

                Option.Complaint = await(
                    from cm in db.Complaint
                    join cu in db.Customer on cm.CustomerId equals cu.Id                    
                    where App.ActiveStatus.Contains(cm.Status)
                    select new
                    {
                        cm.Id,
                        cm.Code,
                        SubText = $"{cm.Date.ToString("dd-MMM-yyyy")}<br>{cu.Description}"
                    }
                ).ToListAsync();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> GetAddOptionAsync(User User)
        {
            Message objMsg = new Message();
            try
            {
                dynamic Option = new ExpandoObject();
                var ListStatus = App.ByPassUserType.Contains(User.UserType) ? App.AllActiveStatus : new List<int>() { App.Status.Scheduled, App.Status.Processing };
                Option.Complaint = await (
                    from cm in db.Complaint
                    join cu in db.Customer on cm.CustomerId equals cu.Id
                    join ad in db.AdminDiv on cm.AdminDivId equals ad.Id
                    join co in db.Country on cm.CountryId equals co.Id
                    where ListStatus.Contains(cm.Status) && cm.CompanyId == User.Id
                    orderby cm.Code
                    select new
                    {
                        cm.Id,
                        cm.Code,
                        cm.Date,
                        CustomerDesc = cu.Description,
                        CustomerAddress = Util.AddressDesc(new Composite.AddressDesc
                        {
                            Add1 = cu.Address1,
                            Add2 = cu.Address2,
                            PinCode = cu.PinCode,
                            PostOffice = cu.PostOffice,
                            District = cu.District,
                            State = ad.Description,
                            Country = co.Description
                        }),
                        cm.Problem,
                        IsItemAdded = db.ComplaintItem.Any(ci => ci.ComplaintId == cm.Id)
                    }
                ).ToListAsync();

                Option.Item = await (
                    from it in db.Item
                    join ut in db.Unit on it.UnitId equals ut.Id
                    where App.ActiveStatus.Contains(it.Status)
                    select new 
                    {
                        ItemId = it.Id,                        
                        ItemDesc = it.Description,                        
                        UnitDesc = ut.Description,
                    }
                ).ToListAsync();
                
                objMsg.data = Option;
                Message.Get(ref objMsg, "");
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<List<ComplaintItem>> ListAsync(ComplaintItem? obj, User User) 
        {
            obj ??= new ComplaintItem();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : App.ActiveStatus;
            var dbCompliantItemQuery = db.ComplaintItem.Where(ci => obj.ListStatus.Contains(ci.Status)).AsQueryable();
            dbCompliantItemQuery = dbCompliantItemQuery.Where(ci => !obj.ListComplaintId.Any() || obj.ListComplaintId.Contains(ci.ComplaintId));
            dbCompliantItemQuery = dbCompliantItemQuery.Where(ci => !obj.ListId.Any() || obj.ListId.Contains(ci.Id));

            var ComplaintItem = await (
                from ci in dbCompliantItemQuery
                join co in db.Complaint on ci.ComplaintId equals co.Id
                join cu in db.Customer on co.CustomerId equals cu.Id
                join ad in db.AdminDiv on co.AdminDivId equals ad.Id
                join ct in db.Country on co.CountryId equals ct.Id
                join it in db.Item on ci.ItemId equals it.Id
                join ut in db.Unit on it.UnitId equals ut.Id
                join st in db.Setting on new { Name = App.SettingName.Status, Value = ci.Status.ToString() } equals new { st.Name, st.Value }
                join cb in db.User on ci.CreatedBy equals cb.Id
                join ub in db.User on ci.UpdatedBy equals ub.Id
                where
                (!obj.ListCustomerId.Any() || obj.ListCustomerId.Contains(co.CustomerId)) &&
                (obj.FromDate == DateTime.MinValue || co.Date.Date >= obj.FromDate.Date) &&
                (obj.ToDate == DateTime.MinValue || co.Date.Date <= obj.ToDate.Date)
                select new ComplaintItem 
                {
                    Id = ci.Id,
                    ComplaintId = ci.ComplaintId,
                    ComplaintNo = co.Code,
                    ComplaintDate = co.Date,
                    CustomerDesc = cu.Description,
                    CustomerAddress = Util.AddressDesc(new Composite.AddressDesc
                    {
                        Add1 = co.Address1,
                        Add2 = co.Address2,
                        PinCode = co.PinCode,
                        PostOffice = co.PostOffice,
                        District = co.District,
                        State = ad.Description,
                        Country = ct.Description,
                        OtherText = co.ContactNo
                    }),
                    Problem = co.Problem,
                    ItemId = ci.ItemId,
                    ItemDesc = it.Description,
                    SerialNo = ci.SerialNo,
                    Rate = ci.Rate,
                    Qty = ci.Qty,
                    Amount = ci.Amount,
                    UnitDesc = ut.Description,
                    CompanyId = ci.CompanyId,
                    Status = ci.Status,
                    StatusDesc = st.Description,
                    StatusCss = st.CssClass,
                    CreatedBy = ci.CreatedBy,
                    CreatedByName = cb.Name,
                    CreatedAt = ci.CreatedAt,
                    UpdatedBy = ci.UpdatedBy,
                    UpdatedByName = ub.Name,
                    UpdatedAt = ci.UpdatedAt,
                    IsEdit = false,
                    IsDelete = ci.Status == App.Status.Enable ? true : false
                }
            ).ToListAsync();            
            return ComplaintItem;
        }
        public async Task<Message> GetAsync(ComplaintItem obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await ListAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> PrintAsync(ComplaintItem obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await ListAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> ExportAsync(ComplaintItem obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                objMsg.data = await ListAsync(obj, User);
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public async Task<Message> AddAsync(List<ComplaintItem> list, User User)
        {
            Message objMsg = new Message();
            try
            {
                //Update Esiting Item
                var UpdateComplaintItem = list.Where(x => x.Id > 0).ToList();
                if (UpdateComplaintItem.Any()) {
                    UpdateComplaintItem.ForEach(ci =>
                    {
                        var obj = list.FirstOrDefault(x => x.Id == ci.Id);
                        if (obj != null)
                        {
                            ci.ItemId = obj.ItemId;
                            ci.Rate = obj.Rate;
                            ci.Qty = obj.Qty;
                            ci.Amount = obj.Amount;
                            ci.UpdatedBy = User.Id;
                            ci.UpdatedAt = DateTime.Now;
                        }
                    });
                    db.UpdateRange(UpdateComplaintItem);
                }
                //Add New Item
                var AddComplaintItem = list.Where(x => x.Id == 0).ToList();
                if (AddComplaintItem.Any()) 
                {
                    AddComplaintItem.ForEach(obj => {
                        obj.Status = App.Status.Enable;                        
                        obj.CreatedBy = User.Id;
                        obj.CreatedAt = DateTime.Now;
                        obj.UpdatedBy = User.Id;
                        obj.UpdatedAt = DateTime.Now;
                    });
                    db.AddRange(AddComplaintItem);
                }                
                int isSaved = await db.SaveChangesAsync();
                Message.Add(ref objMsg, isSaved, "");
                if (objMsg.status == Message.Type.success) 
                {   
                    objMsg.data = await ListAsync(new ComplaintItem
                    {
                        ListId = list.Select(x => x.Id).ToList(),
                        ListStatus = App.ActiveStatus
                    }, User);
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
                var obj = await db.ComplaintItem.FindAsync(Id);
                if (obj == null)
                {
                    Message.Error(ref objMsg, "Complaint Item did not find for delete.!!!");
                    return objMsg;
                }
                obj.Status = App.Status.Delete;
                obj.UpdatedBy = User.Id;
                obj.UpdatedAt = DateTime.Now;
                db.Update(obj);
                int isDelete = await db.SaveChangesAsync();
                Message.Delete(ref objMsg, isDelete, "");
                if (objMsg.status == Message.Type.success) 
                {
                    var dbComplaintItem = await ListAsync(new ComplaintItem
                    {
                        ListId = new List<int> { obj.Id },
                        ListStatus = new List<int> { App.Status.Delete}
                    }, User);
                    objMsg.obj = dbComplaintItem.FirstOrDefault();
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
