class ComplaintStatus {
    static init() {
        ComplaintStatus.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description', });
                Dropdown.bind({ id: '#ListComplaintId', data: response.data.Complaint, value: 'ComplaintId', text: 'ComplaintNo', subText: 'SubText' });
            }
        });
        $('#btnSearch').on('click', () => {
            ComplaintStatus.getComplaint({
                onSuccess: (response) => {
                    Table.add({id:'#tableComplaint', data: response.data});
                }
            });
        });
        $('#Status').on('change', () => {
            if ($('#Status').val() == 4) {
                $('.send-otp').removeClass('hide');
                $('#OTP').addClass('required');
            }
            else {
                $('.send-otp').addClass('hide');
                $('#OTP').removeClass('required');
            }
        });
        $('#btnSendOtp').on('click', () => {
            if (Field.isNullOrEmpty($('#ComplaintId').val())) {
                Message.error({ statusText: "Complaint No. empty" });
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formComplaintStatus" });
            Data.post({
                url: 'ComplaintStatus/SendOtp',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    if (response.status == Message.Type.success) {
                        ComplaintStatus.startOtpTimer({ durationInSeconds: 30, elementId: "#OtpTimer" });
                        
                    }
                }
            });
        });
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formComplaintStatus" });
            ComplaintStatus.add({ obj: obj });
        });
    }
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'ComplaintStatus/GetViewOption', onSuccess: onSuccess });
    }
    static getComplaint({ onSuccess }) {
        let obj = {
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListStatus: $('#ListStatus').val(),
            ListComplaintId: $('#ListComplaintId').val(),
        };
        Data.post({ url: 'ComplaintStatus/GetComplaint', data: obj, onSuccess: onSuccess });
    }
    static newEntry({ obj }) {        
        Data.post({
            url: 'ComplaintStatus/GetAddOption',
            data: obj,
            onSuccess: (response) => {
                Dropdown.bind({ id: '#Status', data: response.data.Status, value: 'Value', text: 'Description' });                
                Modal.open({ id: '#modalComplaintStatus', title: 'Complaint Status / Add', action: 'Add', obj: obj });
                $('.send-otp').addClass('hide');
                ComplaintStatus.stopOtpTimer({ elementId: "#OtpTimer" });
                Table.add({ id: '#tableComplaintStatus', data: response.data.ComplaintStatus });
            }
        });
    }    
    static otpInterval;
    static startOtpTimer({ durationInSeconds, elementId }) {
        let seconds = durationInSeconds;
        const timerElement = document.querySelector(elementId);
        timerElement.innerHTML = `You can request a new OTP after <b>${seconds}</b> seconds.`;
        $('#btnSendOtp').attr('disabled', true);
        ComplaintStatus.otpInterval = setInterval(() => {
            seconds--;
            timerElement.innerHTML = `You can request a new OTP after <b>${seconds}</b> seconds.`;
            if (seconds <= 0) {
                clearInterval(ComplaintStatus.otpInterval);
                timerElement.textContent = "";
                $('#btnSendOtp').attr('disabled', false);
            }
        }, 1000);
    }
    static stopOtpTimer({ elementId }) {
        clearInterval(ComplaintStatus.otpInterval);
        document.querySelector(elementId).textContent = "";        
    }
    static add({ obj }) {
        Data.post({
            url: 'ComplaintStatus/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    $('#Remarks').val(null);
                    let ComplaintStatus = response.data.ComplaintStatus;
                    Table.add({ id: '#tableComplaintStatus', data: ComplaintStatus, action: "prepend" });
                    let Complaint = response.data.Complaint;
                    Table.updateById({ id: '#tableComplaint', objId: Complaint.Id, obj: Complaint });                    
                }
            }
        });
    }
    static delete({ id, index }) {
        Message.confirm({
            msg: "Do you want to delete??",
            confirmButtonText: "Delete",
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({
                    url: `ComplaintStatus/Delete?Id=${id}`,
                    onSuccess: (response) => {
                        Message.show(response);
                        if (response.status == Message.Type.success) {
                            Table.remove({ id: '#tableComplaintStatus', value: [index] });
                        }
                    }
                });
            }
        });
    }
}

window.tableComplaintSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableComplaintInfo = (value, obj, index) => {
    let labelStyle = "width:100px; font-weight: bold;";
    return `
        <div><label style="${labelStyle}">Date</label> <b>:</b> ${moment(obj.Date).format('DD-MMM-YYYY')}</div>
        <div><label style="${labelStyle}">Ticket No.</label> <b>:</b> ${obj.Code}</div>
        <div><label style="${labelStyle}">Customer</label> <b>:</b> ${obj.CustomerDesc.match(/.{1,50}/g).join('<br>')}</div>
        <div><label style="${labelStyle}">Department</label> <b>:</b> ${obj.Department}</div>
        <div><label style="${labelStyle}">Location</label> <b>:</b> ${obj.CustomerLocation}</div>
        <div><label style="${labelStyle}">Contact Person</label> <b>:</b> ${obj.ContactPerson}</div>
        <div><label style="${labelStyle}">Contact No.</label> <b>:</b> ${obj.ContactNo}</div>
        <div><label style="${labelStyle}">Email</label> <b>:</b> ${obj.Email}</div>
        <div class="fw-bold">Problem</div>
        <div>${obj.Problem.match(/.{1,50}/g).join('<br>')}</div>
    `;
}
window.tableComplaintStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableComplaintAction = (value, obj, index) => {
    let actionBtn = [];
    if (obj.IsAddStatus) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-success btn-new-entry" title="New Entry(Add Status)">
                    <span class="fa fa-plus"></span>&nbsp;&nbsp;Add Status
                </a>
            </li>
        `);
    }
    return `
        <div class="btn-group dropstart">
            <button type="button" class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${actionBtn.join('')}
            </ul>
        </div>
    `;     
}
window.tableComplaintActionEvent = {
    'click .btn-new-entry': (e, value, obj, row) => {
        obj.ComplaintId = obj.Id;
        obj.ComplaintNo = obj.Code;
        ComplaintStatus.newEntry({ obj: obj });
    }
}

window.tableComplaintStatusSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableComplaintStatusStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableComplaintStatusLogByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div><div>${moment(obj.CreatedAt).format("DD-MMM-YYYY HH:mm")}</div>`;
}
window.tableComplaintStatusAction = (value, obj, index) => {
    return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableComplaintStatusActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        ComplaintStatus.delete({ id: obj.Id, index: index });
    }
}
