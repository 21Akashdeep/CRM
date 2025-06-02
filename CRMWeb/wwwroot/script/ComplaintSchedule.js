class ComplaintSchedule {
    static init() {
        ComplaintSchedule.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListComplaintId', data: response.data.Complaint, value: 'Id', text: 'Code' });       
            }
        });
        $('#btnSearch').on('click', () => {
            ComplaintSchedule.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableComplaintSchedule', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            ComplaintSchedule.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableComplaintSchedule', data: response.data, isPrint: true, reportDesc: `Complaint Schedule List<br>Generated On: ${moment().format('DD-MMM-YYYY HH:mm')}` });
                }
            });
        });
        $('#btnExport').on('click', () => {
            ComplaintSchedule.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: `Complaint_Schedule_${moment().format('DD_MMM_YYYY_HH_mm')}` });
                }
            });
        });
        $('#btnNewEntry').on('click', () => {
            ComplaintSchedule.newEntry();
        });
        ComplaintSchedule.initAdd();
    }
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'ComplaintSchedule/GetViewOption', onSuccess: onSuccess });
    }
    static get({method, onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListComplaintId: $('#ListComplaintId').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
        };
        Data.post({ url: `ComplaintSchedule/${method}`, data: obj, onSuccess: onSuccess });
    }
    static initAdd() {
        $('#ComplaintId').on('change', () => {
            let obj = Dropdown.itemJson({ id: '#ComplaintId' });
            if (obj) {
                Data.objectToForm({ obj: obj, formId: '#formComplaintSchedule' });
                Table.add({ id: '#tableAssignTo', data: obj.UserList, search: false });
            }
        });
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            else if ($('#tableAssignTo').bootstrapTable('getSelections').length == 0) {
                Message.error({ statusText: 'Select atleat one Service eng.' });
                return;
            }
            let obj = Data.serializeToObject({ formId: '#formComplaintSchedule' });  
            obj.AssignTo = $('#tableAssignTo').bootstrapTable('getSelections');
            if (!obj.Id) {
                ComplaintSchedule.add(obj);
            }
            else {
                ComplaintSchedule.update(obj);
            }
        });
    }
    static newEntry() {
        Data.get({
            url: 'ComplaintSchedule/GetAddOption',
            onSuccess: (response) => {
                Modal.open({ id: '#modalComplaintSchedule', title: 'Complaint Schedule / Add', action: 'Add' });
                response.data.Complaint.map(x => { x.SubText = `<div class="fw-bold">${x.CustomerDesc}</div><div>${x.CustomerAddress}</div>` });
                Dropdown.bind({ id: '#ComplaintId', data: response.data.Complaint, value: 'Id', text: 'Code', subText: 'SubText', json: true });
            }
        });
    }
    static add(obj) {
        Data.post({
            url: 'ComplaintSchedule/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalComplaintSchedule' });
                    Table.add({ id: '#tableComplaintSchedule', data: response.obj, action: 'prepend' });
                    response.data.AddOption.Complaint.map(x => { x.SubText = `<div class="fw-bold">${x.CustomerDesc}</div><div>${x.CustomerAddress}</div>` });
                    Dropdown.bind({ id: '#ComplaintId', data: response.data.AddOption.Complaint, value: 'Id', text: 'Code', subText: 'SubText', json: true });
                }
            }
        });
    }
    static edit({ id }) {
        Data.get({
            url: `ComplaintSchedule/Edit?Id=${id}`,
            onSuccess: (response) => {
                let obj = response.obj;
                response.data.Complaint.map(x => { x.SubText = `<div class="fw-bold">${x.CustomerDesc}</div><div>${x.CustomerAddress}</div>` });
                Dropdown.bind({ id: '#ComplaintId', data: response.data.Complaint, value: 'Id', text: 'Code', subText: 'SubText', json: true });
                let Complaint = response.data.Complaint.find(x => x.Id == obj.ComplaintId);
                Modal.open({ id: '#modalComplaintSchedule', title: 'Complaint Schedule / Edit', action: 'Edit', obj: obj });
                Complaint.UserList.map(x => { x.IsSelected = obj.AssignTo.find(x2 => x2.Id == x.Id) ? true : false; });
                Table.add({ id: '#tableAssignTo', data: Complaint.UserList, search: false });   
            }
        });
    }
    static update(obj) {
        Data.update({
            url: 'ComplaintSchedule/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalComplaintSchedule' });
                    Table.updateById({ id: '#tableComplaintSchedule', objId: response.obj.Id, obj: response.obj });                        
                }
            }
        });
    }
    static delete({ id }) {
        Data.delete({
            url: `ComplaintSchedule/Delete?Id=${id}`,            
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {                    
                    Table.updateById({ id: '#tableComplaintSchedule', objId: response.obj.Id, obj: response.obj });                    
                }
            }
        });
    }
}

window.tableComplaintScheduleSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableComplaintScheduleCustomerDesc = (value, obj, index) => {
    return `
        <div class="fw-bold">${obj.CustomerDesc.match(/.{1,50}/g).join('<br>')}</div>
        <div>${obj.CustomerAddress.match(/.{1,50}/g).join('<br>') }</div>
    `;
}
window.tableComplaintScheduleProblem = (value, obj, index) => {
    return `<div>${obj.Problem.match(/.{1,50}/g).join('<br>')}</div>`;
}
window.tableComplaintScheduleAssignTo = (value, obj, index) => {
    return obj.AssignTo.map(x => { return x.Name }).join('<br>');
}
window.tableComplaintScheduleStartDateTime = (value, obj, index) => {
    return moment(obj.StartDateTime).format('DD-MMM-YYYY HH:mm:ss');
}
window.tableComplaintScheduleEndDateTime = (value, obj, index) => {
    return moment(obj.EndDateTime).format('DD-MMM-YYYY HH:mm:ss');
}
window.tableComplaintScheduleCompletedDateTime = (value, obj, index) => {
    return obj.CompletedDateTime == null ? '-' : moment(obj.CompletedDateTime).format('DD-MMM-YYYY HH:mm:ss');
}
window.tableComplaintScheduleRemarks = (value, obj, index) => {
    return obj.Remarks == null ? '-' : obj.Remarks.match(/.{1,20}/g).join('<br>');
}
window.tableComplaintScheduleCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableComplaintScheduleUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableComplaintScheduleStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableComplaintScheduleAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ? `
                    <li>
                        <a href="#" class="dropdown-item text-success btn-edit" title="View / Edit">
                            <span class="fa fa-edit"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>` : ``
                }                
                ${obj.IsDelete ? `
                    <li>
                        <a href="#" class="dropdown-item text-danger btn-delete" title="Delete">
                            <span class="fa fa-trash"></span>&nbsp;&nbsp;Delete
                        </a>
                    </li>` : ``
                }                
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableComplaintScheduleActionEvent = {
    'click .btn-edit': (e, value, obj, row) => {
        ComplaintSchedule.edit({ id: obj.Id });
    },
    'click .btn-delete': (e, value, obj, row) => {
        ComplaintSchedule.delete({ id: obj.Id });
    },
}

window.tableAssignToIsSelected = (value, obj, index) => {
    return { checked: obj.IsSelected };
}
