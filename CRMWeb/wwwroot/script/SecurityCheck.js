class SecurityCheck {
    static init() {
        $('#ReqNo').on('keydown', (e) => {
            if (e.key == "Enter") {
                SecurityCheck.getReqInfo();               
            }            
        });
        $('#btnReqSearch').on('click', () => {
            SecurityCheck.getReqInfo();       
        });
        $('#btnApprove').on('click', () => {
            SecurityCheck.submit();            
        });        
    }
    static getReqInfo() {   
        let obj = {};
        //Rest Request Form Data
        SecurityCheck.resetUiSate({ ReqNo: $('#ReqNo').val() });
        //Validate Request Data        
        if ($('#ReqNo').val().length >= 12) {            
            obj = SecurityCheck.decodeReqNo($('#ReqNo').val());
            Data.objectToForm({ obj: obj, formId: "#formSecurityCheck" });
        }
        else {
            Message.error({ statusText: 'Invalid Request No.' });            
            return;
        }
        //Get Data From Server
        Data.post({
            url: `SecurityCheck/GetReqInfo`,
            data: obj,
            onSuccess: (response) => {
                if (response.status == Message.Type.success) {
                    if (response.obj.ApiName == App.Setting.ApiName.OutPass) {
                        SecurityCheck.bindOutPass(response);
                    }
                    else {
                        Message.error({ statusText: "Invalid Req. No." });
                    }
                }
                else {
                    Message.show(response);                    
                }                
            }
        });
    }
    static decodeReqNo(ReqNo) {
        let obj = {
            ReqNo: ReqNo,
            ApiName: ReqNo.substring(0, 2) == "OP" ? App.Setting.ApiName.OutPass : App.Setting.ApiName.InPass
        };
        return obj;
    }
    static resetUiSate({ ReqNo = null } = {}) {        
        $('#ReqNo').val(ReqNo);
        $('#ApiName, #Remarks').val(null);
        $('#tableReqInfo').html('');
        $('.req-input').addClass('hide');
    }    
    static bindOutPass(response) {
        let table = [];
        let obj = response.obj;        
        table.push('<table>');
        table.push('<tr><th colspan="2">Out Pass Info</th></tr>');
        table.push(`
                <tr>
                    <th>Out Pass No.</th>
                    <td>${obj.Code}</td>
                </tr>
                <tr>
                    <th>Date</th>
                    <td>${moment(obj.Date).format('DD-MMM-YYYY')}</td>
                </tr>
                <tr>
                    <th>Name Of Person</th>
                    <td>${obj.NameOfPerson}</td>
                </tr>
                <tr>
                    <th>Designation</th>
                    <td>${obj.Designation == null ? "-" : obj.Designation}</td>
                </tr>
                <tr>
                    <th>Representing (Name<br>Of Firm / Contractor etc.)</th>
                    <td>${obj.FirmName}</td>
                </tr>
                <tr>
                    <th>Vehicle No.</th>
                    <td>${obj.VehicleNo == null ? "-" : obj.VehicleNo}</td>
                </tr>
                <tr>
                    <th>Driver Name</th>
                    <td>${obj.DriverName == null ? "-" : obj.DriverName}</td>
                </tr>
                <tr>
                    <th>Sold / Auctioned as<br>salvage / Issued</th>
                    <td>${obj.TransferInfo == null ? "-" : obj.TransferInfo}</td>
                </tr>
                <tr>
                    <th>Is Returnable</th>
                    <td>${obj.IsReturnable ? "Yes" : "No"}</td>
                </tr>
                <tr>
                    <th>Purpose</th>
                    <td>${obj.Purpose}</td>
                </tr>
                <tr>
                    <th>Destination</th>
                    <td>${obj.Destination}</td>
                </tr>
                <tr>
                    <th>Status</th>
                    <td><div class="${obj.StatusCss} mt-1 mb-1">${obj.StatusDesc}</div></td>
                </tr>                
                <tr>
                    <th>Is Out</th>
                    <td><div class="${obj.IsOut ? "bg-success" : "bg-danger"} rounded-5 pt-1 pb-1 ps-3 mt-1 mb-1">${obj.IsOut ? "Yes" : "No"}</div></td>
                </tr>
                <tr>
                    <th>Out Remarks</th>
                    <td>${obj.OutRemarks??"-"}</td>
                </tr>
                <tr>
                    <th>Is Returned</th>
                    <td>
                        <div class="${obj.IsReturned == null || obj.IsReturned ? "bg-success" : "bg-danger"} rounded-5 pt-1 pb-1 ps-3 mt-1 mb-1">
                            ${obj.IsReturned == null ? "Not Applicable" : obj.IsReturned ? "Yes" : "No"}
                        </div>
                    </td>
                </tr>
                <tr>
                    <th>Received Remarks</th>
                    <td>${obj.ReceivedRemarks??"-"}</td>
                </tr>
            `);        
        let itemTable = [];
        $(obj.OutPassItem).each((rowIndex, obj) => {
            itemTable.push(`
                <tr>
                    <td class="text-center">${rowIndex + 1}</td>
                    <td>
                        <div class="row">
                            <div class="col-12"><label class="fw-bold">Item Desc :</label> ${obj.ItemDesc}</div>
                            <div class="col-3"><div class="fw-bold">Primary Unit</div>${obj.PrimaryUnitDesc}</div>
                            <div class="col-3"><div class="fw-bold">Primary Qty.</div>${obj.PrimaryQty}</div>
                            <div class="col-3"><div class="fw-bold">Secondary Unit</div>${obj.SecondaryUnitDesc}</div>
                            <div class="col-3"><div class="fw-bold">Secondary Qty.</div>${obj.SecondaryQty}</div>
                            <div class="col-12"><label class="fw-bold">Remarks : </label>${obj.Remarks == null ? "" : obj.Remarks}</div>
                        </div>
                    </td>
                </tr>
            `);
        });
        table.push(`
                <tr>
                    <td colspan="2">
                        <table class="table table-bordered table-hover mt-2 mb-2">
                            <tr><th class="text-center">#</th><th>Item Info</th></tr>
                            ${itemTable.join('')}
                        </table>
                    </td>
                </tr>                
            `);
        table.push('</table>');        
        $('#tableReqInfo').html(table.join(''));
        $('.req-input').removeClass('hide');
    }
    static submit() {
        let obj = Data.serializeToObject({ formId: "#formSecurityCheck" });        
        Message.confirm({
            msg: "Do you want to submit",
            confirmButtonText: "Submit",
            denyButtonText: "Dont Submit",
            data: obj,
            onConfirm: (obj) => {
                Data.post({
                    url: `SecurityCheck/Submit`,
                    data: obj,
                    onSuccess: (response) => {
                        Message.show(response)
                        if (response.status == Message.Type.success) {
                            SecurityCheck.resetUiSate();
                        }
                    }
                });
            }
        });
    }
}