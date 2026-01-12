class Gdn {
    static item = [];

    static init() {

        Gdn.getViewOption();

        $('#btnNewEntry').on('click', () => {
            Gdn.newEntry();
        });

        $('#btnSearch').on('click', () => {
            Gdn.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableGdn', data: response.data });
                }
            });
        });

        $('#btnPrint').on('click', () => {
            Gdn.get({
                method: 'Print',
                onSuccess: (response) => {
                    Table.add({ id: '#tableGdn', data: response.data, isPrint: true });
                }
            });
        });

        $('#btnExport').on('click', () => {
            Gdn.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: 'Gdn' });
                }
            });
        });

        $('#Gdn-BtnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }

            let GdnItem = $('#tableGdnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);

            if (GdnItem.length == 0) {
                Message.error({ statusText: 'Gdn Item not found. Add atleast one Gdn Item' });
                return;
            }

            let obj = Data.serializeToObject({ formId: '#formGdn' });

            obj.GdnItem = GdnItem;
            obj.NetAmount = GdnItem.reduce((sum, x) => sum + (Number(x.Amount) || 0), 0).toFixed(2);

            if (!obj.Id) {
                Gdn.add(obj);
            }
            else {
                Gdn.update(obj);
            }
        });
    }

    static getViewOption() {
        Data.get({
            url: 'Gdn/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListCustomerId', data: response.data.Customer, value: 'Id', text: 'Name' });
                Dropdown.bind({ id: '#ListConName', data: response.data.ConName, value: 'Id', text: 'ConName' });
            }
        });
    }

    static getAddOption({ onSuccess }) {
        Data.get({ url: 'Gdn/GetAddOption', onSuccess: onSuccess });
    }

    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            ListCustomerId: $('#ListCustomerId').val(),
            ListConName: $('#ListConName').val()
        };

        Data.post({
            url: `Gdn/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }

    static newEntry() {
        Gdn.getAddOption({
            onSuccess: (response) => {
                Gdn.item = response.data.Item;
                Dropdown.bind({ id: '#Gdn-PartyId', data: response.data.Party, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Gdn-ShiftId', data: response.data.Shift, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalGdn', title: 'Gdn / Add', action: 'Add' });
            }
        });
    }
}
window.tableGdnRefNoAndDate = (value, obj, index) => {
    return `
        <div>${obj.RefNo ?? ''}</div>
        <div>${obj.RefDate ? moment(obj.RefDate).format('DD-MMM-YYYY') : ''}</div>
    `;
};

window.tableGdnSlNo = (value, obj, index) => {
    return index + 1;
};

window.tableGdnDate = (value, obj, index) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
};

window.tableGdnStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
};

window.tableGdnCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
};

window.tableGdnUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName}</div>
        <div>${DateTime.dateTime(obj.UpdatedAt)}</div>
    `;
};
window.tableGdnAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsEdit ? `
                <li>
                    <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
                        <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
                    </a>
                </li>` : ``}

                ${obj.IsDuplicate ? `
                <li>
                    <a href="#" class="dropdown-item text-primary-100 btn-duplicate" title="Duplicate">
                        <span class="fa fa-copy text-primary-100"></span>&nbsp;&nbsp;Duplicate
                    </a>
                </li>` : ``}

                ${obj.IsDelete ? `
                <li>
                    <a href="#" class="dropdown-item text-danger-100 btn-delete" title="Delete">
                        <span class="fa fa-trash text-danger-100"></span>&nbsp;&nbsp;Delete
                    </a>
                </li>` : ``}

                ${obj.IsEnable ? `
                <li>
                    <a href="#" class="dropdown-item text-success-100 btn-enable" title="Enable">
                        <span class="fa fa-toggle-on text-success-100"></span>&nbsp;&nbsp;Enable
                    </a>
                </li>` : ``}
            </ul>
        </div>
    `;
    return actionBtn;
};
window.tableGdnConAddress = (value, obj, index) => {

    const parts = [
        obj.ConAdd1,
        obj.ConAdd2,
        obj.ConPostOffice,
        obj.ConPincode,
        obj.ConStateName
    ]
        .filter(x => x && x.trim() !== "")
        .map(x => x.trim());

    if (parts.length === 0) return "";

    let lines = [];
    for (let i = 0; i < parts.length; i += 2) {
        lines.push(parts.slice(i, i + 2).join(", "));
    }

    return lines.join("<br>");
};
window.tableGdnActionEvent = {
    'click .btn-edit': (e, value, obj, index) => {
        Gdn.edit({ id: obj.Id });
    },
    'click .btn-duplicate': (e, value, obj, index) => {
        Gdn.edit({ id: obj.Id, action: 'Add' });
    },
    'click .btn-print': (e, value, obj, index) => {
        Gdn.print({ id: obj.Id });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Gdn.delete({ id: obj.Id });
    }
};
