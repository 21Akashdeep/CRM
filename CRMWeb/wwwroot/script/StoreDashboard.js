class StoreDashboard {
    static mainItem = [];
    static item = [];
    static itemGroup = [];
    static itemSubGroup = [];
    static storeDesc = [];
    static store = [];
    static historyItemId = 0;
    static historyStoreId = 0;
    static init() {

        StoreDashboard.getViewOption();

        $('#btnNewEntry').on('click', () => {
            StoreDashboard.newEntry();
        });

        $('#btnSearch').on('click', () => {
            StoreDashboard.get({
                method: 'Get',
                onSuccess: (response) => {
                    let data = response.data;
                    Table.add({ id: '#tableStoreDashboard', data: response.data });
                }
            });
        });
        $('#btnPrint').on('click', () => {

            StoreDashboard.print(StoreDashboard.mainItem);

        });


        $('#btnExport').on('click', () => {
            StoreDashboard.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: 'StoreDashboard' });
                }
            });
        });
        $('#ListStoreId').on('change', () => {

            let obj = {
                StoreId: $('#ListStoreId').val()
            };
            StoreDashboard.storeDesc = $('#ListStoreId').val();


            if (obj.StoreId) {
                Data.post({
                    url: 'StoreDashboard/Get',
                    data: obj,
                    onSuccess: (response) => {
                        Message.success(response)
                        StoreDashboard.mainItem = response.data.ItemInJamshedpur;
                        let itemGroup = StoreDashboard.itemGroup;
                        Dropdown.bind({ id: '#ListItemGroupId', data: itemGroup, value: 'Id', text: 'Description' });
                        Table.add({ id: '#tableStoreDashboard', data: response.data.ItemInJamshedpur });

                    }

                });
            }
        });

        $('#ListItemGroupId').on('change', () => {
            let itemStoreId = $('#ListStoreId').val();
            let itemgroupId = $('#ListItemGroupId').val();

            let obj = {
                StoreId: itemStoreId,
                ListGroupId: itemgroupId
            }
            StoreDashboard.groupId = itemgroupId;

            if (itemgroupId.length > 0) {

                Data.post({
                    url: 'StoreDashboard/GetDataByItemGroup',
                    data: obj,
                    onSuccess: (response) => {
                        StoreDashboard.mainItem = response.data.ItemInJamshedpur;
                        StoreDashboard.itemSubGroup = response.data.itemSubGroup;
                        Dropdown.bind({ id: '#ListItemSubGroupId', data: StoreDashboard.itemSubGroup, value: 'Id', text: 'Description' });
                        Table.remove({ id: '#tableStoreDashboard' });
                        Table.add({ id: '#tableStoreDashboard', data: response.data.ItemInJamshedpur });
                    }
                });


            }


        });

        $('#ListItemSubGroupId').on('change', () => {
            let itemStoreId = $('#ListStoreId').val();
            let itemSubGroupId = $('#ListItemSubGroupId').val();
            let itemgroupid = $('#ListItemGroupId').val()

            let obj = {
                StoreId: itemStoreId,
                ListGroupId: itemgroupid,
                ListSubGroupId: itemSubGroupId
            }

            if (itemSubGroupId.length > 0) {
                Data.post({
                    url: 'StoreDashboard/GetDataByItemGroupItemSubGroup',
                    data: obj,
                    onSuccess: (response) => {
                        StoreDashboard.mainItem = response.data.ItemInJamshedpur;
                        let itemgroupIds = itemgroupid.map(Number);
                        let itemsubgroupIds = itemSubGroupId.map(Number);
                        let selectItem = StoreDashboard.item.filter(x => itemgroupIds.includes(x.ItemGroupId) && itemsubgroupIds.includes(x.itemSubGroupId));
                        Dropdown.bind({ id: '#ListItemId', data: response.data.item, value: 'Id', text: 'Description' });
                        Table.remove({ id: '#tableStoreDashboard' });
                        Table.add({ id: '#tableStoreDashboard', data: response.data.ItemInJamshedpur });
                    }
                });



            }
        });
        $('#ListItemId').on('change', () => {
            let itemStoreId = $('#ListStoreId').val();
            let itemSubGroupId = $('#ListItemSubGroupId').val();
            let itemgroupid = $('#ListItemGroupId').val();
            let itemId = $('#ListItemId').val();

            let obj = {
                StoreId: itemStoreId,
                ListGroupId: itemgroupid,
                ListSubGroupId: itemSubGroupId,
                ListItemId: itemId
            }

            if (itemId.length > 0) {
                Data.post({
                    url: 'StoreDashboard/GetDataByItemId',
                    data: obj,
                    onSuccess: (response) => {
                        StoreDashboard.mainItem = response.data.ItemInJamshedpur;
                        Table.remove({ id: '#tableStoreDashboard' });
                        Table.add({ id: '#tableStoreDashboard', data: response.data.ItemInJamshedpur });
                    }
                });



            }
        });

        $('#btnHistorySearch').on('click', () => {

            StoreDashboard.getHistory({
                method: 'GetItemHistory'
            });

        });


        $('#btnHistoryPrint').on('click', () => {

            StoreDashboard.getHistory({
                method: 'PrintItemHistory',
                onSuccess: (response) => {

                    Table.add({
                        id: '#tableItemHistory',
                        data: response.data,
                        isPrint: true
                    });

                }
            });

        });


        $('#btnHistoryExport').on('click', () => {

            StoreDashboard.getHistory({
                method: 'ExportItemHistory',
                onSuccess: (response) => {

                    Export.Base64ToExcel({
                        base64: response.base64,
                        fileName: 'ItemHistory'
                    });

                }
            });

        });


    }
    static getViewOption() {
        Data.get({
            url: 'StoreDashboard/GetViewOption',
            onSuccess: (response) => {
                StoreDashboard.item = response.data.Item;
                StoreDashboard.itemGroup = response.data.ItemGroup;
                StoreDashboard.itemSubGroup = response.data.ItemSubGroup;
                StoreDashboard.store = response.data.Store;
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListStoreId', data: response.data.Store, value: 'Id', text: 'Description' });

            }
        });
    }
    static get({ method = 'Get', onSuccess }) {
        let obj = {
            ListStatus: $('#ListStatus').val(),
            StoreId: $('#ListStoreId').val()

        };
        Data.post({
            url: `StoreDashboard/${method}`,
            data: obj,
            onSuccess: onSuccess
        });
    }
    static print(data) {
        let StoreDesc = StoreDashboard.store
            .filter(x => x.Id == StoreDashboard.storeDesc)
            .map(x => x.Description);
        let reportTitle = 'Store Dashboard';

        let list = Array.isArray(data) ? data : [];
        let printContent = [];
        let table = [];

        table.push('<table class="table table-bordered">');

        table.push(`
        <thead>
            <tr>
                <th colspan="6">
                    <div class="text-center brand-name">${App.Company.Description}</div>
                    <div class="title-row">
        <div class="report-title text-center">${reportTitle}</div>
        <div class="store-name ">Location: ${StoreDesc}</div>
         <div class="store-name text-right ">Generated At : ${moment().format('DD-MMM-YYYY HH:mm')}</div>
    </div>
                </th>
            </tr>
            <tr>
                <th>SL No.</th>
                <th>Material</th>
                <th>Material Type</th>
                <th>Make</th>
                <th class="text-center">Qty</th>
            </tr>
        </thead>
    `);

        table.push('<tbody>');

        list.forEach((row, index) => {

            table.push(`
            <tr>
                <td class="text-center">${index + 1}</td>
                <td>${row.ItemGroupDesc || '-'}</td>
                <td>${row.ItemSubGroupDesc || '-'}</td>
                <td>${row.ItemDesc || '-'}</td>
                <td class="text-center">${row.TotalQty || 0}</td>
            </tr>
        `);

        });

        let totalQty = list.reduce((s, x) => s + Number(x.TotalQty || 0), 0);

        table.push(`
        <tr>
            <th colspan="4" class="text-right">Total</th>
            <th class="text-center">${totalQty}</th>
        </tr>
    `);

        table.push('</tbody>');
        table.push('</table>');

        printContent.push(`
       <div class="page-container">
       <div class="page">

        ${table.join('')}

        <div class="print-footer not-print" style="text-align:center;margin-top:20px;">
            <button onclick="window.print()" 
            style="padding:8px 25px;background:white;color:black;border:2px solid black;border-radius:4px;cursor:pointer;">
                Print
            </button>
        </div>

      </div>
      </div>
      `);


        let style = [];
        style.push(`
        table { border-collapse: collapse; width: 100%; }
        table td, table th { border: 1px solid black; padding: 5px; }
        .text-right { text-align: right; }
        .text-center { text-align: center; }
    `);

        Print.page({
            title: reportTitle,
            style: style,
            content: printContent,
            border: false,
            orientation: 'A4 landscape',
            isPrint: false
        });

    }
    static edit({ id }) {

        let storeId = $('#ListStoreId').val();

        StoreDashboard.historyItemId = id;
        StoreDashboard.historyStoreId = storeId;

        StoreDashboard.getHistoryOption();

        StoreDashboard.getHistory({
            method: 'GetItemHistory'
        });

    }
    static getHistoryOption() {

        Data.get({
            url: 'StoreDashboard/GetHistoryViewOption',
            onSuccess: (response) => {

                Dropdown.bind({
                    id: '#HistoryVoucherType',
                    data: response.data.VoucherType,
                    value: 'Value',
                    text: 'Description'
                });

            }
        });

    }
    static getHistory({ method = 'GetItemHistory', onSuccess }) {

        let obj = {
            ItemId: StoreDashboard.historyItemId,
            StoreId: StoreDashboard.historyStoreId,
            SerialNo: $('#HistorySerialNo').val(),
            VoucherType: $('#HistoryVoucherType').val(),
            DateRange: $('#HistoryDateRange').val()
        };

        Data.get({
                url: `StoreDashboard/${method}?itemId=${obj.ItemId}&storeId=${obj.StoreId}`,
            data: obj,
            onSuccess: (response) => {

                let data = response.data;

                if (data && data.length > 0) {

                    let d = data[0];

                    $('#historyItemGroup').text(d.ItemGroup || '');
                    $('#historyItemSubGroup').text(d.ItemSubGroup || '');
                    $('#historyItemName').text(d.ItemName || '');

                    Table.remove({ id: '#tableItemHistory' });

                    Table.add({
                        id: '#tableItemHistory',
                        data: data
                    });

                    if (!onSuccess)
                        Modal.open({ id: '#modalItemHistory' });

                    if (onSuccess)
                        onSuccess(response);
                }
            }
        });

    }
}
window.tableStoreDashboardSLNo = (value, obj, index) => {
    return index + 1;
}
   
window.tableStoreDashboardAction = (value, obj, index) => {
    return `
        <div class="btn-group dropstart">            
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>

            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">

                <li>
                    <a href="#" class="dropdown-item text-success btn-edit">
                        <span class="fa fa-edit"></span>&nbsp;&nbsp;View
                    </a>
                </li>

            </ul>
        </div>
    `;
}

window.tableStoreDashboardActionEvent = {

    'click .btn-edit': (e, value, obj, index) => {
        StoreDashboard.edit({ id: obj.ItemId });
    }

}
window.tableItemHistorySLNo = (v, obj, index) => {
    return index + 1;
};
window.tableItemHistoryDate = (v, obj) => {
    return moment(obj.Date).format('DD-MMM-YYYY');
};
window.tableItemHistoryCreatedAt = (v, obj) => {
    return moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm');
};
window.tableItemHistorySerial = (value, obj) => {

    if (!obj.SerialNo) return '';

    let serials = obj.SerialNo
        .split(',')
        .map(x => x.trim())
        .filter(x => x.length > 0);

    return `
        <ul class="mb-0 ps-3">
            ${serials.map(s => `<li>${s}</li>`).join('')}
        </ul>
    `;
};
window.tableItemHistoryQtyIn = (v) => {
    return `<span class="text-success">${v}</span>`;
}

window.tableItemHistoryQtyOut = (v) => {
    return `<span class="text-danger">${v}</span>`;
}