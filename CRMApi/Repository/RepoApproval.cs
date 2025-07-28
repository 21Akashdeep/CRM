using DocumentFormat.OpenXml.Spreadsheet;
using CRMApi.Models;
using CRMApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Xml;

namespace CRMApi.Repository
{
    public class RepoApproval
    {
        private readonly DBCRM db;
        private AppSetting App = Util.AppSetting;
        public RepoApproval(DBCRM _db) 
        {
            db = _db;
        }
        public Message GetViewOption() 
        {
            Message objMsg = new Message();
            try 
            {
                dynamic Option = new ExpandoObject();
                List<string> ApprovalStatus = App.ApprovalStatus.Select(x => x.ToString()).ToList();
                ApprovalStatus.Add(App.Status.Delete.ToString());
                Option.Status = db.Setting.Where(st => App.ActiveStatus.Contains(st.Status) && st.Name == App.SettingName.Status && 
                ApprovalStatus.Contains(st.Value)).Select(st => new
                {
                    st.Value,
                    st.Description
                }).ToList();
                Option.Api = db.Api.Where(ap => App.ActiveStatus.Contains(ap.Status) && ap.IsApprovalRequired).Select(ap => new
                {
                    ap.Id,
                    ap.Code,
                    ap.Name,
                    ap.Description
                }).ToList();
                Option.Req = db.Approval.Where(ap => App.ApprovalStatus.Contains(ap.Status))
                    .GroupBy(ap => new { ap.ReqId, ap.ReqNo })
                    .Select(ap => new
                    {
                        ap.Key.ReqId,
                        ap.Key.ReqNo
                    }).ToList();
                objMsg.data = Option;
                Message.Success(ref objMsg, "Record found");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public List<Approval> List(Approval? obj, User User)
        {
            obj ??= new Approval();
            obj.ListStatus = obj.ListStatus.Any() ? obj.ListStatus : new List<int> { App.Status.WaitingForApproval };
            var dbApproval = db.Approval.Where(ap => obj.ListStatus.Contains(ap.Status)).ToList();
            dbApproval = obj.ListId.Any() ? dbApproval.Where(ap => obj.ListId.Contains(ap.Id)).ToList() : dbApproval;
            dbApproval = obj.ListApiId.Any() ? dbApproval.Where(ap => obj.ListApiId.Contains(ap.ApiId)).ToList() : dbApproval;
            dbApproval = obj.ListReqId.Any() ? dbApproval.Where(ap => obj.ListReqId.Contains(ap.ReqId)).ToList() : dbApproval;
            dbApproval = obj.ListReqNo.Any() ? dbApproval.Where(ap => obj.ListReqNo.Contains(ap.ReqNo)).ToList() : dbApproval;
            dbApproval = obj.ListUserId.Any() && User.UserType != App.UserType.SysAdmin ? 
                         dbApproval.Where(ap => obj.ListUserId.Contains(ap.UserId)).ToList() : dbApproval;
            dbApproval = obj.FromDate != default(DateTime) ? dbApproval.Where(ap => ap.ReqDate >= obj.FromDate).ToList() : dbApproval;
            dbApproval = obj.TillDate != default(DateTime) ? dbApproval.Where(ap => ap.ReqDate <= obj.TillDate).ToList() : dbApproval;

            var Approval = (
                from apr in dbApproval
                join api in db.Api on apr.ApiId equals api.Id
                join usr in db.User on apr.UserId equals usr.Id                
                join sts in db.Setting on new { Value = apr.Status.ToString(), Name = App.SettingName.Status } equals new { sts.Value, sts.Name }
                select new Approval
                {
                    Id = apr.Id,
                    ApprovalConfigId = apr.ApprovalConfigId,
                    ApiId = apr.ApiId,
                    ApiName = api.Name,
                    ReqId = apr.ReqId,
                    ReqNo = apr.ReqNo,
                    ReqDate = apr.ReqDate,
                    NetAmount = apr.NetAmount,
                    IsLastApprover = apr.IsLastApprover,
                    SeqNo = apr.SeqNo,
                    UserId = apr.UserId,
                    UserName = usr.Name,
                    UserEmail = usr.Email,                    
                    Remarks = apr.Remarks,
                    Status = apr.Status,
                    StatusDesc = sts.Description,
                    StatusCss = sts.CssClass,
                    IsView = true,
                    IsApprove = apr.Status == App.Status.WaitingForApproval ? true : false,
                    IsReject = apr.Status == App.Status.WaitingForApproval ? true : false,
                }
            ).ToList();
            Approval = !String.IsNullOrEmpty(obj.ApiName) ? Approval.Where(ap => ap.ApiName == obj.ApiName).ToList() : Approval;
            return Approval;
        }
        public Message GetReqInfo(Approval obj, User User)
        {
            Message objMsg = new Message();
            try 
            {
                if (obj.ApiName == App.ApiName.OutPass)
                {
                    List<int> ApprovalStatus = App.ApprovalStatus;
                    ApprovalStatus.Add(App.Status.Delete);
                    if (obj.ApiName == App.ApiName.OutPass)
                    {
                        //objMsg.data = new RepoOutPass(db).List(new OutPass { ListId = obj.ListReqId, ListStatus = ApprovalStatus }, User);
                        Message.Get(ref objMsg, "");
                    }
                    else 
                    {
                        Message.Error(ref objMsg, "Invalid Api Name.");
                    }
                }
                else 
                {
                    Message.Error(ref objMsg, "Record did not find");
                }
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Add(Approval obj, User User) 
        {
            Message objMsg = new Message();
            try
            {
                var dbApprovalConfig = db.ApprovalConfig.Where(ac => App.ActiveStatus.Contains(ac.Status) && ac.ApiId == obj.ApiId).AsEnumerable().FirstOrDefault();
                if (dbApprovalConfig == null) 
                {
                    Message.Error(ref objMsg, $"Approval Config did not find for Api ({obj.ApiName})");
                    return objMsg;
                }
                // Get all the User-Roles mapping
                var dbUser = db.User.Where(ur => App.ActiveStatus.Contains(ur.Status)).ToList();
                var dbUserApRo = db.UserApprovalRole.Select(x => new { x.Id, x.ApprovalRoleId }).ToList();                
               
                // Get Approval Config mapping with roles
                var ApprovalConfig = (
                    from ac in dbApprovalConfig.ApprovalSeq
                    join ur in dbUserApRo on ac.ApprovalRoleId equals ur.ApprovalRoleId
                    select new 
                    {
                        dbApprovalConfig.Id,
                        ac.ApprovalRoleId,
                        ac.SeqNo,
                        UserId = ur.Id
                    }
                ).ToList();
                
                // Modify ApprovalConfig sequence based on the first approver
                if (obj.FirstApproverId > 0) 
                {                                      
                    ApprovalConfig = ApprovalConfig.Where(ac => ac.SeqNo == 1 && ac.UserId == obj.FirstApproverId).ToList()
                                    .Union(ApprovalConfig.Where(ac => ac.SeqNo > 1).ToList()).ToList();
                }
                var FirstApproverSeq = ApprovalConfig.Min(x => x.SeqNo);
                var LastApproverSeq = ApprovalConfig.Max(x => x.SeqNo);
                
                // Create Approval records
                var Approval = ApprovalConfig.Select(ac => new Approval
                {
                    ApprovalConfigId = ac.Id,
                    ApiId = User.ApiId,
                    ReqId = obj.ReqId,
                    ReqNo = obj.ReqNo,
                    ReqDate = obj.ReqDate,                                        
                    NetAmount = 0,
                    IsLastApprover = ac.SeqNo == LastApproverSeq,
                    SeqNo = ac.SeqNo,
                    UserId = ac.UserId,
                    Status = ac.SeqNo == FirstApproverSeq ? App.Status.WaitingForApproval : App.Status.Pending,
                    CreatedBy = User.Id,
                    UpdatedBy = User.Id
                }).ToList();
                db.AddRange(Approval);
                Message.Add(ref objMsg, db.SaveChanges(), "");
                objMsg.tranId = Approval.Where(ap => ap.SeqNo == FirstApproverSeq).Select(ap => ap.Id).ToList();
            } 
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Delete(Approval obj, User User) 
        {
            Message objMsg = new Message();
            try 
            {
                var DeleteApproval = db.Approval.Where(x => App.UnderProcess.Contains(x.Status) && x.ApiId == User.ApiId && x.ReqId == obj.ReqId).ToList();
                //Assign Approval Id
                objMsg.tranId = DeleteApproval.Where(x => x.Status == App.Status.WaitingForApproval).Select(x => x.Id).ToList();
                //Delete Approval
                DeleteApproval.ForEach(x =>
                {
                    x.Status = App.Status.Delete;
                    x.UpdatedBy = User.Id;
                    x.UpdatedAt = DateTime.Now;
                });
                db.UpdateRange(DeleteApproval);                
                Message.Success(ref objMsg, "Approval Ready for delete.");
            }
            catch (Exception ex) 
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }
        public Message Approve(Approval obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbApproval = db.Approval.Where(x => App.UnderProcess.Contains(x.Status) && x.ReqId == obj.ReqId).ToList();
                var UpdateApproval = dbApproval.FirstOrDefault(x => x.Id == obj.Id && x.UserId == User.Id);
                if (UpdateApproval == null)
                {
                    Message.Error(ref objMsg, "Approval record did not find for approve.<br>Or, You are not authorised to approve this record.");
                    return objMsg;
                }
                var transaction = db.Database.CreateExecutionStrategy();
                transaction.Execute(() =>
                {
                    db.Database.BeginTransaction();
                    try 
                    {
                        //Update Approval
                        UpdateApproval.Remarks = obj.Remarks;
                        UpdateApproval.Status = App.Status.Approved;
                        UpdateApproval.UpdatedBy = User.Id;
                        UpdateApproval.UpdatedAt = DateTime.Now;
                        db.Update(UpdateApproval);
                        //Update Cuurent SeqNo Approver Status
                        var UpdateCurSeqApproval = dbApproval.Where(x => x.SeqNo == UpdateApproval.SeqNo && x.Id != UpdateApproval.Id).ToList();
                        UpdateCurSeqApproval.ForEach(x =>
                        {
                            x.Status = App.Status.AutoApproved;
                            x.UpdatedBy = User.Id;
                            x.UpdatedAt = DateTime.Now;
                        });
                        db.UpdateRange(UpdateCurSeqApproval);
                        //Active Next SeqNo
                        var UpdateNextApproval = dbApproval.Where(x => x.SeqNo == UpdateApproval.SeqNo + 1).ToList();
                        UpdateNextApproval.ForEach(x =>
                        {
                            x.Status = App.Status.WaitingForApproval;
                            x.UpdatedBy = User.Id;
                            x.UpdatedAt = DateTime.Now;
                        });
                        db.UpdateRange(UpdateNextApproval);
                        Message.Approve(ref objMsg, db.SaveChanges(), "");
                        if (objMsg.status == Message.Type.success) 
                        {
                            if (obj.ApiName == App.ApiName.OutPass)
                            {
                                //Update Outpass                    
                                //Message msg = new RepoOutPass(db).UpdateStatus(new OutPass
                                //{
                                //    Id = obj.ReqId,
                                //    ApprovalRemarks = UpdateApproval.Remarks,
                                //    ListApprovalId = UpdateApproval.IsLastApprover ? new List<int> { 0 } : UpdateNextApproval.Select(x => x.Id).ToList(),
                                //    IsLastApprover = UpdateApproval.IsLastApprover,
                                //    Status = UpdateApproval.IsLastApprover ? App.Status.Approved : App.Status.Processing,
                                //}, User);
                                //if (msg.status != Message.Type.error)
                                //{
                                //    db.Database.CommitTransaction();
                                //}
                                //else
                                //{
                                //    objMsg.status = Message.Type.warning;
                                //    objMsg.statusText = $"{objMsg.statusText}<br>{msg.statusText}";
                                //    db.Database.RollbackTransaction();
                                //}
                            }
                        }
                    }
                    catch (Exception ex) 
                    {
                        Message.Exception(ref objMsg, ex);
                        db.Database.RollbackTransaction();
                    }    
                });                
                //Get Data & Option
                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = List(new Approval
                    {
                        ListId = new List<int> { UpdateApproval.Id },
                        ListStatus = new List<int> { App.Status.Approved}
                    }, User).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
                }
            }
            catch (Exception ex)
            {
                Message.Exception(ref objMsg, ex);
            }
            return objMsg;
        }        
        public Message Reject(Approval obj, User User)
        {
            Message objMsg = new Message();
            try
            {
                var dbApproval = db.Approval.Where(x => App.UnderProcess.Contains(x.Status) && x.ReqId == obj.ReqId).ToList();
                var UpdateApproval = dbApproval.FirstOrDefault(x => x.Id == obj.Id && x.UserId == User.Id);
                if (UpdateApproval == null)
                {
                    Message.Error(ref objMsg, "Approval record did not find for approve.");
                    return objMsg;
                }
                var transaction = db.Database.CreateExecutionStrategy();
                transaction.Execute(() =>
                {
                    db.Database.BeginTransaction();
                    try 
                    {
                        //Update Approval
                        UpdateApproval.Remarks = obj.Remarks;
                        UpdateApproval.Status = App.Status.Reject;
                        UpdateApproval.UpdatedBy = User.Id;
                        UpdateApproval.UpdatedAt = DateTime.Now;
                        db.Update(UpdateApproval);
                        //Active Next SeqNo
                        var UpdateNextApproval = dbApproval.Where(x => x.SeqNo > UpdateApproval.SeqNo).ToList();
                        UpdateNextApproval.ForEach(x =>
                        {
                            x.Remarks = obj.Remarks;
                            x.Status = App.Status.AutoReject;
                            x.UpdatedBy = User.Id;
                            x.UpdatedAt = DateTime.Now;
                        });
                        db.UpdateRange(UpdateNextApproval);
                        Message.Reject(ref objMsg, db.SaveChanges(), "");
                        if (objMsg.status == Message.Type.success)
                        {
                            //Update Status In Document 
                            if (obj.ApiName == App.ApiName.OutPass)
                            {
                                ////Update Outpass                    
                                //Message msg = new RepoOutPass(db).UpdateStatus(new OutPass
                                //{
                                //    Id = obj.ReqId,
                                //    ApprovalRemarks = obj.Remarks,
                                //    Status = App.Status.Reject,
                                //}, User);
                                //if (msg.status != Message.Type.error)
                                //{
                                //    db.Database.CommitTransaction();
                                //}
                                //else
                                //{
                                //    objMsg.status = Message.Type.warning;
                                //    objMsg.statusText = $"{objMsg.statusText}<br>{msg.statusText}";
                                //    db.Database.RollbackTransaction();
                                //}
                            }
                        }
                    }
                    catch (Exception ex) 
                    {
                        Message.Exception(ref objMsg, ex);
                        db.Database.RollbackTransaction();
                    }
                });
                
                //Get Data & Option
                if (objMsg.status == Message.Type.success)
                {
                    objMsg.obj = List(new Approval { ListId = new List<int> { UpdateApproval.Id } }, User).FirstOrDefault();
                    objMsg.data = GetViewOption().data;
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
