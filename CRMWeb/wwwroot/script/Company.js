class Company {
    //Company List
    static init() {
        Company.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.Company, value: 'Id', text: 'Description', subText: "Code" });
        });
        $('#btnSearch').on('click', () => {
            Company.get({
                action: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableCompany', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {
            Company.get({
                action: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableCompany', data: response.data, isPrint: true, reportDesc: 'Company List' });
                }
            });
        });
        $('#btnExport').on('click', () => {
            Company.get({
                action: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "Company" });
                }
            });
        });
        $('#btnAdd').on('click', () => {
            Company.fill();
        });
        Company.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Company/GetViewOption', onSuccess: onSuccess });
    }
    static get({ action, onSuccess }) {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post({ url: `Company/${action}`, data: obj, onSuccess: onSuccess });
    }

    //Company Add
    static initAdd() {
        $('#Company_PinCode').on('input', () => {
            let PinCode = $('#Company_PinCode').val();
            OnlineApi.pinCode({
                pinCode: PinCode,
                postOfficeId: "#Company_PostOffice",
                districtId: "#Company_District",
                stateId: "#Company_AdminDivId",
                countryId: "#Company_CountryId"
            });
        });
        $('#Company_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".company-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formCompany" });
            if (!obj.Id) {
                Company.add(obj);
            }
            else {
                Company.update(obj);
            }
        });

    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Company/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Company.getAddOption((response) => {
            Modal.open({ id: '#modalCompany', title: 'Company / Add', action: 'Add' });
            $('#Company_SeqNo').val(0);
            Dropdown.bind({ id: '#Company_CountryId', data: response.data.Country, value: 'Id', text: 'Description' });
            Dropdown.bind({ id: '#Company_AdminDivId', data: response.data.AdminDiv, value: 'Id', text: 'Description' });

        });
    }
    static add(obj) {
        Data.post(
            {
                url: 'Company/Add',
                data: obj,
                onSuccess: Company.addOnSuccess
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalCompany" });
            Table.add({ id: "#tableCompany", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.Company, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `Company/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Company / Edit (Code: ${obj.Code})` : `Company / Add`;
                        Dropdown.bind({ id: '#Company_CountryId', data: response.data.Country, value: 'Id', text: 'Description', subText: 'SubText' });
                        Dropdown.bind({ id: '#Company_AdminDivId', data: response.data.AdminDiv, value: 'Id', text: 'Description' });
                        Modal.open({ id: '#modalCompany', title: title, action: action, obj: obj });
                        OnlineApi.pinCode({
                            pinCode: obj.PinCode,
                            postOffice: obj.PostOffice,
                            district: obj.District,
                            state: obj.AdmivDivId,
                            country: obj.CountryId,
                            pinCodeId: "#Company_PinCode",
                            postOfficeId: "#Company_PostOffice",
                            districtId: "#Company_District",
                            stateId: "#Company_AdminDivId",
                            countryId: "#Company_CountryId",
                        });
                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update(
            {
                url: 'Company/Update',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    Company.updateOnSuccess(response);
                }
            }
        );
    }
    static updateOnSuccess = (response) => {
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalCompany" });
            Table.updateById({ id: "#tableCompany", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Company, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static delete({ id }) {
        Message.confirm(
            {
                msg: 'Do you want to delete??',
                confirmButtonText: 'Delete',
                denyButtonText: 'Dont Delete',
                data: id,
                onConfirm: (id) => {
                    Data.delete({ url: `Company/Delete?Id=${id}`, onSuccess: Company.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableCompany", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Company, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        }
    }
    static enable({ id }) {
        Message.confirm(
            {
                msg: 'Do you want to enable??',
                confirmButtonText: 'Enable',
                denyButtonText: 'Dont Enable',
                data: id,
                onConfirm: (id) => {
                    Data.update({ url: `Company/Enable?Id=${id}`, onSuccess: Company.enableOnSuccess });

                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableCompany", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Company, value: 'Id', text: 'Description', subText: "Code" });
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
        }
    }
}
window.tableCompanySLNo = (value, obj, index) => {
    return index + 1;
}
window.tableCompanyAddress = (value, obj, index) => {
    let address = [];
    if (obj.Address1 != null && obj.Address1.trim() !== "") {
        address.push(obj.Address1.trim());
    }
    if (obj.Address2 != null && obj.Address2.trim() !== "") {
        address.push(obj.Address2.trim());
    }

    if (obj.PostOffice != null && obj.PostOffice.trim() !== "") {
        address.push(obj.PostOffice.trim());
    }
    if (obj.District != null && obj.District.trim() !== "") {
        address.push(obj.District.trim());
    }
    if (obj.CountryDesc != null && obj.CountryDesc.trim() !== "") {
        address.push(obj.CountryDesc.trim());
    }
    if (obj.AdminDivDesc != null && obj.AdminDivDesc.trim() !== "") {
        address.push(obj.AdminDivDesc.trim());
    }
    if (obj.PinCode != null && obj.PinCode.trim() !== "") {
        address.push(obj.PinCode.trim());
    }

    let fullAddress = address.join(', ');
    return fullAddress.match(/.{1,30}/g)?.join('<br>') || '';
};
window.tableCompanyName = (value, obj, index) => {
    return obj.Name.match(/.{1,30}/g).join('<br>');;
}
window.tableCompanyDescription = (value, obj, index) => {
    return obj.Description.match(/.{1,30}/g).join('<br>');;
}
tableCompanyStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}<div>`;
}
tableCompanyCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
tableCompanyUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
tableCompanyAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ?
            `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                        </a>
                    </li>` : ``
        }
                ${obj.IsDuplicate ?
            `<li>
                        <a href="#" class="dropdown-item text-primary-100 btn-duplicate" title="Duplicate">
                            <span class="fa fa-copy text-primary-100"></span>&nbsp;&nbsp;Duplicate
                        </a>
                    </li>` : ``
        }
                ${obj.IsDelete ?
            `<li>
                        <a href="#" class="dropdown-item text-danger-100 btn-delete" title="Delete">
                            <span class="fa fa-trash text-danger-100"></span>&nbsp;&nbsp;Delete
                        </a>
                    </li>` : ``
        }
                ${obj.IsEnable ?
            `<li>
                        <a href="#" class="dropdown-item text-success-100 btn-enable" title="Enable">
                            <span class="fa fa-toggle-on text-success-100"></span>&nbsp;&nbsp;Enable
                        </a>
                    </li>` : ``
        }
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableCompanyActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Company.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Company.edit({ id: obj.Id, action: 'Add' })
    },
    'click .btn-delete': (e, value, obj, index) => {
        Company.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Company.enable({ id: obj.Id });
    }
}
