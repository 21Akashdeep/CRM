class Complaint {    
    static init() {
        Complaint.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListCustomerId', data: response.data.Customer, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#ListId', data: response.data.Complaint, value: 'Id', text: 'Code' });                
            }
        });
        $('#btnSearch').on('click', () => {
            Complaint.get({
                method:'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableComplaint', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Complaint.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableComplaint', data: response.data, isPrint: true, reportDesc: `Complaint List Generated On : ${moment().format('DD-MMM-YYYY HH:mm')}` });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Complaint.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: `Complaint_${moment().format('DD_MMM_YYYY_HH_mm')}` });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            Complaint.newEntry();
        });
        Complaint.initAdd();
    }
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'Complaint/GetViewOption', onSuccess: onSuccess });
    }
    static get({ method, onSuccess }) {
        let obj = {
            ListId: $('#ListId').val(),
            ListCustomerId: $('#ListCustomerId').val(),
            ListStatus: $('#ListStatus').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
        };
        Data.post({
            url: `Complaint/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static initAdd() {
        $('#Complaint_CustomerId').on('change', () => {
            const obj = Dropdown.itemJson({ id: '#Complaint_CustomerId' });
            if (!obj) return;            
            $('#Complaint_Address1').val(obj.Address1);
            $('#Complaint_Address2').val(obj.Address2);            
            OnlineApi.pinCode({
                pinCode: obj.PinCode,
                postOffice: obj.PostOffice,
                district: obj.District,
                state: obj.AdminDivId,
                country: obj.CountryId,
                pinCodeId:'#Complaint_PinCode',
                postOfficeId: "#Complaint_PostOffice",
                districtId: "#Complaint_District",
                stateId: "#Complaint_AdminDivId",
                countryId: "#Complaint_CountryId",
            });
            Field.triggerOnInput('#Complaint_Address1, #Complaint_PostOffice, #Complaint_District');
            Field.triggerOnChange('#Complaint_AdminDivId, #Complaint_CountryId');
        });
        $('#Complaint_PinCode').on('input', () => {            
            let PinCode = $('#Complaint_PinCode').val();
            OnlineApi.pinCode({
                pinCode: PinCode,
                postOfficeId: "#Complaint_PostOffice",
                districtId: "#Complaint_District",
                stateId: "#Complaint_AdminDivId",
                countryId: "#Complaint_CountryId",
            });            
        });
        $('#Complaint_ContactNo').on('focusout', () => {
            if ($('#Complaint_ContactNo').val().length < 10) {
                Message.error({ statusText: 'Mobile No. should be 10 digit.' });
            }
        });
        $('#Complaint_DepartmentId').on('change', () => {
            if ($('#Complaint_DepartmentId').val()) {
                let obj = {
                    DepartmentId: $('#Complaint_DepartmentId').val()
                };
                Complaint.getAddOption({
                    obj: obj,
                    onSuccess: (response) => {
                        Dropdown.bind({ id: '#Complaint_ForwardTo', data: response.data.ForwardTo, value: 'Id', text: 'Name', subText: 'SubText' });
                    }
                });
            }
            else {
                Dropdown.bind({ id: '#Complaint_ForwardTo', data: [], value: 'Id', text: 'Name', subText: 'SubText' });
            }            
        });
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.complaint-required' })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: '#formComplaint' });                      
            if (obj.ForwardTo) {
                Message.confirm({
                    msg: 'Do you want save & Assign to supervisor.',
                    confirmButtonText: 'Save',
                    denyButtonText: "Don't Save",
                    data: obj,
                    onConfirm: (obj) => {
                        if (!obj.Id) {
                            Complaint.add(obj);
                        }
                        else {
                            Complaint.update(obj);
                        }
                    }
                });
            }
            else {
                if (!obj.Id) {
                    Complaint.add(obj);
                }
                else {
                    Complaint.update(obj);
                }
            }            
        });        
    }
    static getAddOption({ obj = {}, onSuccess }) {
        Data.post({ url: 'Complaint/GetAddOption', data: obj, onSuccess: onSuccess });
    }
    static newEntry() {
        Complaint.getAddOption({
            onSuccess: (response) => {
                Modal.open({ id: '#modalComplaint', title: 'Complaint / Add', action: 'Add' });
                Dropdown.bind({ id: '#Complaint_SupportMode', data: response.data.SupportMode, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Complaint_CustomerId', data: response.data.Customer, value: 'Id', text: 'Description', json: true });
                Dropdown.bind({ id: '#Complaint_AdminDivId', data: response.data.AdminDiv, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Complaint_CountryId', data: response.data.Country, value: 'Id', text: 'Description', initialValue: [App.Info.CountryId] });
                Dropdown.bind({ id: '#Complaint_DepartmentId', data: response.data.Department, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Complaint_ForwardTo', data: response.data.ForwardTo, value: 'Id', text: 'Name', subText: 'SubText' });
                Dropdown.bind({ id: '#Complaint_Priority', data: response.data.Priority, value: 'Id', text: 'Description' });
                
            }
        });        
    }    
    static add(obj) {
        Data.post({
            url: 'Complaint/Add',
            data: obj,
            onSuccess: Complaint.addOnSuccess
        });        
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: '#modalComplaint' });
            Table.add({ id: '#tableComplaint', data: response.obj, action: 'prepend' });
        }
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `Complaint/Edit?Id=${id}`,
            onSuccess: (response) => {                
                //Bind Option
                let Option = response.data;                
                Dropdown.bind({ id: '#Complaint_SupportMode', data: Option.SupportMode, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Complaint_CustomerId', data: Option.Customer, value: 'Id', text: 'Description', json: true });
                Dropdown.bind({ id: '#Complaint_AdminDivId', data: Option.AdminDiv, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Complaint_CountryId', data: Option.Country, value: 'Id', text: 'Description', initialValue: [App.Info.CountryId] });
                Dropdown.bind({ id: '#Complaint_DepartmentId', data: Option.Department, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Complaint_Priority', data: Option.Priority, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Complaint_ForwardTo', data: Option.ForwardTo, value: 'Id', text: 'Name', subText: 'SubText' });                
                //Assing Value in Form
                let obj = response.obj;                
                obj.Id = action == "Edit" ? obj.Id : null;                
                let title = action == "Edit" ? `Complaint / Edit (Complaint No. : ${obj.Code})` : `Complaint / Add`;
                Modal.open({ id: '#modalComplaint', title: title, action: action, obj: obj });                
                Table.add({ id: '#tableComplaintItem', data: obj.ComplaintItem, search: false, selectPick: true, overflow: 'visible', height: 'auto', mobileResponsive: false });
                setTimeout(() => {
                    OnlineApi.pinCode({
                        pinCode: obj.PinCode,
                        postOffice: obj.PostOffice,
                        district: obj.District,
                        state: obj.AdminDivId,
                        country: obj.Country,
                        postOfficeId: "#Complaint_PostOffice",
                        districtId: "#Complaint_District",
                        stateId: "#Complaint_AdminDivId",
                        countryId: "#Complaint_CountryId",
                        loader: true
                    });
                }, 100);
                
            }
        });
    }
    static update(obj) {
        Data.update({
            url: 'Complaint/Update',
            data: obj,
            onSuccess: Complaint.updateOnSuccess
        }); 
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: '#modalComplaint' });
            Table.updateById({ id: '#tableComplaint', objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });            
            Dropdown.bind({ id: '#ListId', data: response.data.Complaint, value: 'Id', text: 'Code' });                
        }
    }
    static delete({ id }) {
        Message.confirm({
            msg: 'Do you want to delete',
            confirmButtonText: 'Delete',
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({ url: `Complaint/Delete?Id=${id}`, onSuccess: Complaint.deleteOnSuccess })
            }
        });        
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: '#tableComplaint', objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });            
            Dropdown.bind({ id: '#ListId', data: response.data.Complaint, value: 'Id', text: 'Code' });                
        }
    }
    static deleteItem({ id, index}) {
        Message.confirm({
            msg: 'Do you want to delete',
            confirmButtonText: 'Delete',
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({
                    url: `Complaint/DeleteItem?Id=${id}`,
                    onSuccess: (response) => {
                        Message.show(response);
                        if (response.status == Message.Type.success) {
                            Table.remove({ id: '#tableComplaintItem', value: [index] });
                        }
                    }
                });
            }
        });        
    }    
}
//Complaint
window.tableComplaintSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableComplaintDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableComplaintCustomerDesc = (value, obj, index) => {    
    return `
        <div class="fw-bold">${obj.CustomerDesc.match(/.{1,40}/g).join('<br>')}</div>
        <div>${obj.CustomerAddress.match(/.{1,40}/g).join('<br>')}</div>         
    `;
}
window.tableComplaintAssingTo = (value, obj, index) => {
    return obj.AssignTo.length > 0 ? obj.AssignTo.map(x => x.AssignToName).join('<br>') : '-';
}
window.tableComplaintCustomerProblem = (value, obj, index) => {
    return `<div>${Field.isNullOrEmpty(obj.Problem) ? '-' : obj.Problem.match(/.{1,50}/g).join('<br>')}</div >`;
}
window.tableComplaintStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableComplaintCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableComplaintUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableComplaintAction = (value, obj, index) => {
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
                ${obj.IsDuplicate ? `
                    <li>
                        <a href="#" class="dropdown-item text-primary btn-duplicate" title="Duplicate">
                            <span class="fa fa-copy"></span>&nbsp;&nbsp;Duplicate
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
                ${obj.IsEnable ? `
                    <li>
                        <a href="#" class="dropdown-item text-success btn-enable" title="Enable">
                            <span class="fa fa-toggle-on"></span>&nbsp;&nbsp;Enable
                        </a>
                    </li>` : ``
                }                
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableComplaintActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Complaint.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Complaint.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Complaint.delete({ id: obj.Id});
    }
}
