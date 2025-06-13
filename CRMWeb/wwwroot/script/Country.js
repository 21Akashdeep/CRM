class Country {
    //Country List
    static init() {
        Country.getViewOption((response) => {
            Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#ListId', data: response.data.Country, value: 'Id', text: 'Description', subText: "Code" });
        });
        $('#btnSearch').on('click', () => {
            Country.get();
        });
        $('#btnPrint').on('click', () => {
            Country.print();
        });
        $('#btnExport').on('click', () => {
            Country.export();
        });
        $('#btnAdd').on('click', () => {
            Country.fill();
        });
        Country.initAdd();
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Country/GetViewOption', onSuccess: onSuccess });
    }
    static get() {
        var obj = {
            ListStatus: $('#ListStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'Country/Get',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableCountry', data: response.data });
                }
            }
        );
    }
    static print() {
        var obj = {
            ListRecordStatus: $('#ListRecordStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'Country/Print',
                data: obj,
                onSuccess: (response) => {
                    Table.add({ id: '#tableCountry', data: response.data, isPrint: true });
                }
            }
        );
    }
    static export() {
        var obj = {
            ListRecordStatus: $('#ListRecordStatus').val(),
            ListId: $('#ListId').val()
        }
        Data.post(
            {
                url: 'Country/Export',
                data: obj,
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fielName: "Country" });
                }
            }
        );
    }
    //Country Add
    static initAdd() {
        $('#Country_PinCode').on('input', () => {
            let PinCode = $('#Country_PinCode').val();
            OnlineApi.pinCode({
                pinCode: PinCode,
                postOfficeId: "#Country_PostOffice",
                districtId: "#Country_District",
                stateId: "#Country_AdminDivType",
                PostalType: "#Country_PostalType"
            });
        });
        $('#Country_btnSave').on('click', () => {
            if (!Field.isMandatory({ class: ".country-required" })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formCountry" });
            if (!obj.Id) {
                
                Country.add(obj);
            }
            else {
                Country.update(obj);
            }
        });
    }
    static getAddOption(onSuccess) {
        Data.get({ url: 'Country/GetAddOption', onSuccess: onSuccess });
    }
    static fill() {
        Country.getAddOption((response) => {
            Modal.open({ id: '#modalCountry', title: 'Country / Add', action: 'Add' });
            $('#Country_SeqNo').val(0);
            Dropdown.bind({ id: '#Country_AdminDivType', data: response.data.AdminDivType, value: 'Value', text: 'Description' });
            Dropdown.bind({ id: '#Country_PostalType', data: response.data.PostalType, value: 'Value', text: 'Description' });
        });
    }
    static add(obj) {
        Data.post(
            {
                url: 'Country/Add',
                data: obj,
                onSuccess: Country.addOnSuccess
            }
        );
    }
    static addOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.reset({ id: "#modalCountry" });
            Table.add({ id: "#tableCountry", data: response.obj, action: 'prepend' });
            Dropdown.bind({ id: '#ListId', data: response.data.Country, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
    static edit({ id, action = "Edit" }) {
        Data.get(
            {
                url: `Country/Edit?Id=${id}`,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.obj;
                        obj.Id = action == 'Edit' ? obj.Id : null;
                        let title = action == 'Edit' ? `Country / Edit (Code: ${obj.Code})` : `Country / Add`;
                        Modal.open({ id: '#modalCountry', title: title, action: action, obj: obj });
                        Dropdown.bind({ id: '#Country_AdminDivType', data: response.data.AdminDivType, value: 'Value', text: 'Description'});
                        Dropdown.bind({ id: '#Country_PostalType', data: response.data.PostalType, value: 'Value', text: 'Description' });

                    }
                    else {
                        Message.show(response);
                    }
                }
            }
        );
    }
    static update(obj) {
        Data.update({ url: 'Country/Update', data: obj, onSuccess: Country.updateOnSuccess });
    }
    static updateOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Modal.close({ id: "#modalCountry" });
            Table.updateById({ id: "#tableCountry", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Country, value: 'Id', text: 'Description', subText: "Code" });
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
                    Data.delete({ url: `Country/Delete?Id=${id}`, onSuccess: Country.deleteOnSuccess });
                }
            }
        );
    }
    static deleteOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableCountry", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Country, value: 'Id', text: 'Description', subText: "Code" });
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
                    Data.update({ url: `Country/Enable?Id=${id}`, onSuccess: Country.enableOnSuccess });

                }
            }
        );
    }
    static enableOnSuccess = (response) => {
        Message.show(response);
        if (response.status == Message.Type.success) {
            Table.updateById({ id: "#tableCountry", objId: response.obj.Id, obj: response.obj });
            Dropdown.bind({ id: '#ListId', data: response.data.Country, value: 'Id', text: 'Description', subText: "Code" });
        }
    }
}
window.tableCountrySLNo = (value, obj, index) => {
    return index + 1;
}
window.tableCountryName = (value, obj, index) => {
    return obj.Name.match(/.{1,30}/g).join('<br>');;
}
window.tableCountryStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusName}</div>`;
}
window.tableCountryCreatedByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div>
            <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableCountryUpdatedByAndAt = (value, obj, index) => {
    return `<div>${obj.UpdatedByName}</div>
            <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm:ss')}</div>`;
}
window.tableCountryAction = (value, obj, index) => {
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
window.tableCountryActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Country.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Country.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Country.delete({ id: obj.Id });
    },
    'click .btn-enable': (e, value, obj, index) => {
        Country.enable({ id: obj.Id });
    },
}