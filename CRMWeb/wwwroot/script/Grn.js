class Grn {
    static init() {
        Grn.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListCustomerId', data: response.data.Customer, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#ListPartyId', data: response.data.Customer, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#ListId', data: response.data.Grn, value: 'Id', text: 'Code' });
            }
        });
        $('#btnSearch').on('click', () => {
            Grn.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableGrn', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Grn.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableGrn', data: response.data, isPrint: true, reportDesc: `Grn List Generated On : ${moment().format('DD-MMM-YYYY HH:mm')}` });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Grn.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: `Grn_${moment().format('DD_MMM_YYYY_HH_mm')}` });
                }
            });
        });
        $('#btnNewEntry').on('click', () => {
            Grn.newEntry();
        });
        Grn.initAdd();
    }
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'Grn/GetViewOption', onSuccess: onSuccess });
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
            url: `Grn/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static initAdd() {
        Customer.initAdd();
        Customer.addOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalCustomer' });
            Grn.getAddOption({
                onSuccess: (response) => {
                    Dropdown.bind({ id: '#Grn_CustomerId', data: response.data.Customer, value: 'Id', text: 'Description', json: true, initialValue: [obj.Id] });
                    Field.triggerOnChange('#Grn_CustomerId');
                }
            });
        }
        Customer.updateOnSuccess = (response) => {
            let obj = response.obj;
            Modal.close({ id: '#modalCustomer' });
            setTimeout(() => {
                Grn.getAddOption({
                    onSuccess: (response) => {
                        Dropdown.bind({ id: '#Grn_CustomerId', data: response.data.Customer, value: 'Id', text: 'Description', json: true, initialValue: [obj.Id] });
                        Field.triggerOnChange('#Grn_CustomerId');
                    }
                });
            }, 500);
        }
        $('#Grn_CustomerId').on('change', () => {
            const obj = Dropdown.itemJson({ id: '#Grn_CustomerId' });
            if (!obj) return;
            $('#Grn_Address1').val(obj.Address1);
            $('#Grn_Address2').val(obj.Address2);
            OnlineApi.pinCode({
                pinCode: obj.PinCode,
                postOffice: obj.PostOffice,
                district: obj.District,
                state: obj.AdminDivId,
                country: obj.CountryId,
                pinCodeId: '#Grn_PinCode',
                postOfficeId: "#Grn_PostOffice",
                districtId: "#Grn_District",
                stateId: "#Grn_AdminDivId",
                countryId: "#Grn_CountryId",
            });
            $('#Grn_ContactNo').val(obj.ContactNo);
            $('#Grn_Email').val(obj.Email);
            $('#Grn_LocationDesc').val(obj.LocationDesc);
            Field.triggerOnInput('#Grn_Address1, #Grn_PostOffice, #Grn_District');
            Field.triggerOnChange('#Grn_AdminDivId, #Grn_CountryId');
            Grn.getAddOption({
                obj: { CustomerId: obj.Id },
                onSuccess: (response) => {
                    Table.add({ id: '#tableGrnAssign', data: response.data.GrnAssing });
                }
            });
        });
        $('#Grn_PinCode').on('input', () => {
            let PinCode = $('#Grn_PinCode').val();
            OnlineApi.pinCode({
                pinCode: PinCode,
                postOfficeId: "#Grn_PostOffice",
                districtId: "#Grn_District",
                stateId: "#Grn_AdminDivId",
                countryId: "#Grn_CountryId",
            });
        });
        $('#Grn_ContactNo').on('focusout', () => {
            if ($('#Grn_ContactNo').val().length < 10) {
                Message.error({ statusText: 'Mobile No. should be 10 digit.' });
            }
        });
        $('#Grn_DepartmentId').on('change', () => {
            if ($('#Grn_DepartmentId').val()) {
                let obj = {
                    DepartmentId: $('#Grn_DepartmentId').val()
                };
                Grn.getAddOption({
                    obj: obj,
                    onSuccess: (response) => {
                        Dropdown.bind({ id: '#Grn_ForwardTo', data: response.data.ForwardTo, value: 'Id', text: 'Name', subText: 'SubText' });
                    }
                });
            }
            else {
                Dropdown.bind({ id: '#Grn_ForwardTo', data: [], value: 'Id', text: 'Name', subText: 'SubText' });
            }
        });
        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.Grn-required' })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: '#formGrn' });
            if (!obj.Id) {
                Grn.add(obj);
            }
            else {
                Grn.update(obj);
            }
        });
    }
    static getAddOption({ obj = {}, onSuccess }) {
        Data.post({ url: 'Grn/GetAddOption', data: obj, onSuccess: onSuccess });
    }
    static newEntry() {
        Grn.getAddOption({
            onSuccess: (response) => {
                Modal.open({ id: '#modalGrn', title: 'Grn / Add', action: 'Add' });
                Dropdown.bind({ id: '#Grn_SupportMode', data: response.data.SupportMode, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Grn_CustomerId', data: response.data.Customer, value: 'Id', text: 'Description', json: true });
                Dropdown.bind({ id: '#Grn_AdminDivId', data: response.data.AdminDiv, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Grn_CountryId', data: response.data.Country, value: 'Id', text: 'Description', initialValue: [App.Info.CountryId] });
                Dropdown.bind({ id: '#Grn_Department', data: response.data.Department, value: 'Value', text: 'Description', isEditable: true });
                Dropdown.bind({ id: '#Grn_Priority', data: response.data.Priority, value: 'Id', text: 'Description' });
            }
        });
    }
    static add(obj) {
        Data.post({
            url: 'Grn/Add',
            data: obj,
            onSuccess: Grn.addOnSuccess
        });
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: '#modalGrn' });
            Table.add({ id: '#tableGrn', data: response.obj, action: 'prepend' });
        }
    }
    static edit({ id, action = 'Edit' }) {
        Data.get({
            url: `Grn/Edit?Id=${id}`,
            onSuccess: (response) => {
                //Bind Option
                let Option = response.data;
                Dropdown.bind({ id: '#Grn_SupportMode', data: Option.SupportMode, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Grn_CustomerId', data: Option.Customer, value: 'Id', text: 'Description', json: true });
                Dropdown.bind({ id: '#Grn_AdminDivId', data: Option.AdminDiv, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Grn_CountryId', data: Option.Country, value: 'Id', text: 'Description', initialValue: [App.Info.CountryId] });
                Dropdown.bind({ id: '#Grn_Department', data: Option.Department, value: 'Value', text: 'Description', isEditable: true });
                Dropdown.bind({ id: '#Grn_Priority', data: Option.Priority, value: 'Id', text: 'Description' });
                //Assing Value in Form
                let obj = response.obj;
                obj.Id = action == "Edit" ? obj.Id : null;
                let title = action == "Edit" ? `Grn / Edit (Grn No. : ${obj.Code})` : `Grn / Add`;
                Modal.open({ id: '#modalGrn', title: title, action: action, obj: obj });
                setTimeout(() => {
                    OnlineApi.pinCode({
                        pinCode: obj.PinCode,
                        postOffice: obj.PostOffice,
                        district: obj.District,
                        state: obj.AdminDivId,
                        country: obj.Country,
                        postOfficeId: "#Grn_PostOffice",
                        districtId: "#Grn_District",
                        stateId: "#Grn_AdminDivId",
                        countryId: "#Grn_CountryId",
                        loader: true
                    });
                }, 100);
                Table.add({ id: '#tableGrnAssign', data: response.data.GrnAssing });
            }
        });
    }

    static update(obj) {
        Data.update({
            url: 'Grn/Update',
            data: obj,
            onSuccess: Grn.updateOnSuccess
        });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: '#modalGrn' });
            Table.updateById({ id: '#tableGrn', objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.Grn, value: 'Id', text: 'Code' });
        }
    }
    static close({ id }) {
        Data.update({
            url: `Grn/Close?Id=${id}`,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Table.updateById({ id: '#tableGrn', objId: response.obj.Id, obj: response.obj });
                    Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                    Dropdown.bind({ id: '#ListId', data: response.data.Grn, value: 'Id', text: 'Code' });
                }
            }
        });
    }
    static delete({ id }) {
        Message.confirm({
            msg: 'Do you want to delete',
            confirmButtonText: 'Delete',
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({ url: `Grn/Delete?Id=${id}`, onSuccess: Grn.deleteOnSuccess })
            }
        });
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: '#tableGrn', objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.Grn, value: 'Id', text: 'Code' });
        }
    }
    static deleteItem({ id, index }) {
        Message.confirm({
            msg: 'Do you want to delete',
            confirmButtonText: 'Delete',
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({
                    url: `Grn/DeleteItem?Id=${id}`,
                    onSuccess: (response) => {
                        Message.show(response);
                        if (response.status == Message.Type.success) {
                            Table.remove({ id: '#tableGrnItem', value: [index] });
                        }
                    }
                });
            }
        });
    }
}
//Grn
window.tableGrnSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableGrnDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
}
window.tableGrnCustomerDesc = (value, obj, index) => {
    return `
        <div class="fw-bold text-truncate ch-40" title="${obj.CustomerDesc}">${obj.CustomerDesc}</div>
        <div class="fw-bold text-truncate ch-40" title="Location : ${obj.CustomerLocation}, Contact Person : ${obj.ContactPerson}, Department : ${obj.Department}">
        ${obj.CustomerLocation}, ${obj.ContactPerson}, ${obj.Department}</div>
        <div class="text-truncate ch-40" title="${obj.CustomerAddress}">${obj.CustomerAddress.trim()}</div>
    `;
}
window.tableGrnProblem = (value, obj, index) => {
    return `<div>${Field.isNullOrEmpty(obj.Problem) ? '-' : obj.Problem.match(/.{1,50}/g).join('<br>')}</div >`;
}
window.tableGrnStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableGrnCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableGrnUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableGrnAction = (value, obj, index) => {
    let actionBtn = [];
    if (obj.IsEdit) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-success btn-edit" title="View / Edit">
                    <span class="fa fa-edit"></span>&nbsp;&nbsp;View / Edit
                </a>
            </li>`
        );
    }
    if (obj.IsDuplicate) {
        actionBtn.push(`
             <li>
                <a href="#" class="dropdown-item text-primary btn-duplicate" title="Duplicate">
                    <span class="fa fa-copy"></span>&nbsp;&nbsp;Duplicate
                </a>
            </li>
        `);
    }
    if (obj.IsClose) {
        actionBtn.push(`
             <li>
                <a href="#" class="dropdown-item text-danger btn-close-Grn" title="Duplicate">
                    <span class="fa fa-times"></span>&nbsp;&nbsp;Close
                </a>
            </li>
        `);
    }
    if (obj.IsDelete) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-danger btn-delete" title="Delete">
                    <span class="fa fa-trash"></span>&nbsp;&nbsp;Delete
                </a>
            </li>
        `);
    }
    if (obj.IsEnable) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-success btn-enable" title="Enable">
                    <span class="fa fa-toggle-on"></span>&nbsp;&nbsp;Enable
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
window.tableGrnActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Grn.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Grn.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-close-Grn': (e, value, obj, index) => {
        Grn.close({ id: obj.Id });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Grn.delete({ id: obj.Id });
    },
    'click .btn-detailed-view': (e, value, obj, index) => {
        Grn.detailedView({ id: obj.Id, index: index });
    }
}


//Grn Assing
window.tableGrnAssignCheck = (value, obj, index) => {
    let checkBox = [];
    checkBox.push('<div class="form-switch" style="min-height:auto!important;">');
    checkBox.push('<input class="form-check-input is-location-checked" type="checkbox" id="IsLocationChecked_' + index + '"');
    checkBox.push(obj.IsAdded ? ' checked/>' : '/>');
    checkBox.push('</div>');
    return checkBox.join('');
}
window.tableGrnAssignCheckEvent = {
    'change .is-location-checked': (e, value, obj, index) => {
        obj.IsAdded = $(e.target).is(':Checked') ? true : false;
        Table.updateByIndex({ id: '#tableGrnAssign', index: index, obj: obj, toggle: true });
    }
}