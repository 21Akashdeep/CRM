class ComplaintAssign {
    static init() {
        ComplaintAssign.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListComplaintId', data: response.data.Complaint, value: 'ComplaintId', text: 'ComplaintNo' });
            }
        });
        $('#btnSearch').on('click', () => {
            ComplaintAssign.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableComplaintAssign', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            ComplaintAssign.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableComplaintAssign', data: response.data, isPrint: true, reportDesc: `Complaint Assign List<br>Generated On: ${moment().format('DD-MMM-YYYY HH:mm')}`, printTitle: 'Complaint Assign' });
                }
            });
        });
        $('#btnExport').on('click', () => {
            ComplaintAssign.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: `Complaint_Assign_${moment().format('DD_MMM_YYYY_HH_mm')}` });
                }
            });
        });
        $('#btnNewEntry').on('click', () => {
            ComplaintAssign.newEntry();
        });
        ComplaintAssign.initAdd();
    }
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'ComplaintAssign/GetViewOption', onSuccess: onSuccess });
    }
    static get({method, onSuccess }) {
        let obj = {            
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListComplaintId: $('#ListComplaintId').val(),      
            ListStatus:$('#ListStatus').val()
        };
        Data.post({ url: `ComplaintAssign/${method}`, data: obj, onSuccess: onSuccess });
    }
    static initAdd() {
        $('#ComplaintId').on('change', () => {
            let obj = Dropdown.itemJson({ id: '#ComplaintId' });
            if (obj) {                
                Data.objectToForm({ obj: obj, formId: '#formComplaintAssign' });
                Data.post({
                    url: 'ComplaintAssign/GetUser',
                    data: obj,
                    onSuccess: (response) => {
                        Table.add({ id: '#tableAssignTo', data: response.data, search: false });
                    }
                });                
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
            let obj = Data.serializeToObject({ formId: '#formComplaintAssign' });  
            obj.AssignTo = $('#tableAssignTo').bootstrapTable('getSelections');
            ComplaintAssign.add(obj);            
        });
    }
    static newEntry() {
        Data.get({
            url: 'ComplaintAssign/GetComplaint',
            onSuccess: (response) => {
                Modal.open({ id: '#modalComplaintAssign', title: 'Complaint Assign / Add', action: 'Add' });
                if (response.data.length > 0) {
                    response.data.map(x => { x.SubText = `<div class="fw-bold">${x.CustomerDesc}</div><div>${x.CustomerAddress}</div>` });
                }                
                Dropdown.bind({ id: '#ComplaintId', data: response.data, value: 'Id', text: 'Code', subText: 'SubText', json: true });
            }
        });
    }
    static add(obj) {
        Data.post({
            url: 'ComplaintAssign/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalComplaintAssign' });
                    $('#btnSearch').trigger('click');
                    response.data.Complaint.map(x => { x.SubText = `<div class="fw-bold">${x.CustomerDesc}</div><div>${x.CustomerAddress}</div>` });
                    Dropdown.bind({ id: '#ComplaintId', data: response.data.Complaint, value: 'Id', text: 'Code', subText: 'SubText', json: true });
                }
            }
        });
    }
    static edit({ complaintId }) {
        Data.get({
            url: `ComplaintAssign/Edit?ComplaintId=${complaintId}`,
            onSuccess: (response) => {                
                response.data.Complaint.map(x => { x.SubText = `<div class="fw-bold">${x.CustomerDesc}</div><div>${x.CustomerAddress}</div>` });
                Dropdown.bind({ id: '#ComplaintId', data: response.data.Complaint, value: 'Id', text: 'Code', subText: 'SubText', json: true, initialValue: [complaintId] });
                let Complaint = response.data.Complaint.find(x => x.Id == complaintId);
                Modal.open({ id: '#modalComplaintAssign', title: 'Complaint Assign / Edit', action: 'Edit', obj: Complaint });                
                Table.add({ id: '#tableAssignTo', data: response.data.AssignedUser, search: false });
            }
        });
    }    
    static delete({ id }) {
        Data.delete({
            url: `ComplaintAssign/Delete?Id=${id}`,            
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {                    
                    Table.updateById({ id: '#tableComplaintAssign', objId: response.obj.Id, obj: response.obj });
                    Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                }
            }
        });
    }
}

window.tableComplaintAssignSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableComplaintAssignComplaintDate = (value, obj, index) => {
    return `        
        <div>${moment(obj.ComplaintDate).format('DD-MMM-YYYY')}</div>
    `;
}
window.tableComplaintAssignCustomerDesc = (value, obj, index) => {
    return `
        <div class="fw-bold">${obj.CustomerDesc.match(/.{1,40}/g).join('<br>')}</div>
        <div>${obj.CustomerAddress.match(/.{1,40}/g).join('<br>') }</div>
    `;
}
window.tableComplaintAssignProblem = (value, obj, index) => {
    return `<div>${obj.Problem.match(/.{1,50}/g).join('<br>')}</div>`;
}
window.tableComplaintAssignAssignTo = (value, obj, index) => {
    return obj.AssignToName;
}
window.tableComplaintAssignStartDateTime = (value, obj, index) => {
    return `
        <div>${moment(obj.StartDateTime).format('DD-MMM-YYYY HH:mm:ss')}</div>        
    `;
}
window.tableComplaintAssignEndDateTime = (value, obj, index) => {
    return `        
        <div>${moment(obj.EndDateTime).format('DD-MMM-YYYY HH:mm:ss')}</div>
    `;
}
window.tableComplaintAssignRemarks = (value, obj, index) => {
    return obj.Remarks == null ? '-' : obj.Remarks.match(/.{1,20}/g).join('<br>');
}
window.tableComplaintAssignCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableComplaintAssignStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableComplaintAssignAction = (value, obj, index) => {
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
window.tableComplaintAssignActionEvent = {
    'click .btn-edit': (e, value, obj, row) => {
        ComplaintAssign.edit({ complaintId: obj.ComplaintId });
    },
    'click .btn-delete': (e, value, obj, row) => {
        ComplaintAssign.delete({ id: obj.Id });
    },
}

window.tableAssignToIsSelected = (value, obj, index) => {
    return { checked: obj.IsSelected };
}
