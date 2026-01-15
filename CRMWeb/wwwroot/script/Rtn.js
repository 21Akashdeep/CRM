//class Rtn {
//    static item = [];
//    static reasonCode = [];
//    static store = [];
//    static stateList = [];

//    static init() {

//        Rtn.getViewOption();

//        $('#btnNewEntry').on('click', () => {
//            Rtn.newEntry();
//        });

//        $('#btnSearch').on('click', () => {
//            Rtn.get({
//                method: 'Get',
//                onSuccess: (response) => {
//                    Table.add({ id: '#tableRtn', data: response.data });
//                }
//            });
//        });

//        $('#btnPrint').on('click', () => {
//            Rtn.get({
//                method: 'Print',
//                onSuccess: (response) => {
//                    Table.add({ id: '#tableRtn', data: response.data, isPrint: true });
//                }
//            });
//        });

//        $('#btnExport').on('click', () => {
//            Rtn.get({
//                method: 'Export',
//                onSuccess: (response) => {
//                    Export.Base64ToExcel({ base64: response.base64, fileName: 'Rtn' });
//                }
//            });
//        });

//        $('#Rtn-ConPinCode').on('input', () => {
//            let pinCode = $('#Rtn-ConPinCode').val();
//            if (pinCode.length == 6) {
//                OnlineApi.pinCode({
//                    pinCode: pinCode,
//                    postOfficeId: '#Rtn-ConPostOffice',
//                    stateId: '#Rtn-ConStateName'
//                })
//            }
//        })

//        $('#Rtn-BtnSave').on('click', () => {
//            if (!Field.isMandatory({ class: '.required' })) {
//                return;
//            }

//            let RtnItem = $('#tableRtnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);

//            if (RtnItem.length == 0) {
//                Message.error({ statusText: 'Rtn Item not found. Add at least one Rtn Item' });
//                return;
//            }

//            let obj = Data.serializeToObject({ formId: '#formRtn' });
//            obj.VoucherItem = RtnItem;
//            obj.NetAmount = RtnItem.reduce((sum, x) => sum + (Number(x.Amount) || 0), 0).toFixed(2);

//            if (!obj.Id) {
//                Rtn.add(obj);
//            }
//            else {
//                Rtn.update(obj);
//            }
//        });
//    }

//    static getViewOption() {
//        Data.get({
//            url: 'Rtn/GetViewOption',
//            onSuccess: (response) => {
//                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
//                Dropdown.bind({ id: '#ListPartyId', data: response.data.Party, value: 'Id', text: 'Name' });
//                Dropdown.bind({ id: '#ListNo', data: response.data.Number, value: 'Id', text: 'No' });
//            }
//        });
//    }

//    static getAddOption({ onSuccess }) {
//        Data.get({ url: 'Rtn/GetAddOption', onSuccess: onSuccess });
//    }

//    static get({ method = 'Get', onSuccess }) {
//        let obj = {
//            ListStatus: $('#ListStatus').val(),
//            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
//            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
//            //ListPartyId: $('#ListPartyId').val(),
//            ListNo: $('#ListNo').val()
//        };
//        Data.post({
//            url: `Rtn/${method}`,
//            data: obj,
//            onSuccess: onSuccess
//        });
//    }

//    static newEntry() {
//        Rtn.getAddOption({
//            onSuccess: (response) => {
//                Rtn.item = response.data.Item;
//                Rtn.reasonCode = response.data.ReasonCode;
//                let store = response.data.Store;
//                Dropdown.bind({ id: '#Rtn-GdnNoId', data: response.data.GdnNo, value: 'Id', text: 'No' });
//                Dropdown.bind({ id: '#Rtn-Store', data: store, value: 'Id', text: 'Description' });
//                Modal.open({ id: '#modalRtn', title: 'Rtn / Add', action: 'Add' });
//            }
//        });
//    }

//    static addRtnItem() {
//        let obj = {
//            RtnId: 0,
//            ItemId: 0,
//            VoucherId: 0,
//            StoreId: 0,
//            ItemDesc: null,
//            SerialNo: "",
//            BatchNo: "",
//            ExpiryOn: null,
//            Qty: 0,
//            Rate: 0,
//            UnitDesc: null,
//            Amount: 0,
//            DiscountAmount: 0,
//            TotalAmount: 0,
//            ListTax: "",
//            TaxRate: 0,
//            TaxAmount: 0,
//            GrossAmount: 0,
//            ImageUrl: null,
//            ReasonCode: null,
//            Remarks: null
//        };
//        Table.add({ id: '#tableRtnItem', data: obj, action: 'append', selectPick: true });
//        Rtn.sumOfTotalRtnItem();
//    }

//    static sumOfTotalRtnItem() {
//        let RtnItem = $('#tableRtnItem').bootstrapTable('getData').filter(x => x.ItemId > 0);
//        let Qty = RtnItem.length == 0 ? 0 : RtnItem.map(x => parseFloat(x.Qty)).reduce((s, v) => s + v, 0);

//        let tfoot = `
//            <tfoot>
//                <tr>
//                    <th class="text-right" colspan="4">Total</th>
//                    <th class="text-right">${_Number.format({ num: Qty, dp: 3 })}</th>                    
//                </tr>
//            </tfoot>
//        `;
//        $('#tableRtnItem tfoot').remove();
//        $('#tableRtnItem').append(tfoot);
//    }

//    static add(obj) {
//        Data.post({
//            url: 'Rtn/Add',
//            data: obj,
//            onSuccess: (response) => {
//                Message.show(response);
//                if (response.status == Message.Type.success) {
//                    Modal.reset({ id: '#modalRtn' });
//                    Table.add({ id: '#tableRtn', data: response.obj, action: 'prepend' });
//                }
//            }
//        });
//    }

//    static edit({ id, action = 'Edit' }) {
//        Data.get({
//            url: `Rtn/Edit?Id=${id}`,
//            onSuccess: (response) => {
//                let option = response.data;
//                Rtn.item = option.Item;
//                Rtn.reasonCode = response.data.ReasonCode;
//                let store = response.data.Store;
//                let state = response.data.State;
//                let obj = response.obj;
//                let StoreId = obj.VoucherItem[0].StoreId;
//                obj.StoreId = StoreId;
//                obj.Id = action == 'Edit' ? obj.Id : null;

//                let title = action == 'Edit' ? `Rtn / Edit (Code: ${obj.No})` : `Rtn / Add`;

//                //Dropdown.bind({ id: '#Rtn-ConStateCode', data: state, value: 'Code', text: 'Code', initialValue: [state.Code] });
//                //Dropdown.bind({ id: '#Rtn-ConStateName', data: state, value: 'Name', text: 'Name', initialValue: [state.Name] });
//                //Dropdown.bind({ id: '#Rtn-PartyId', data: response.data.Party, value: 'Id', text: 'Name' });
//                //Dropdown.bind({ id: '#Rtn-Type', data: response.data.Type, value: 'Value', text: 'Description' });
//                Dropdown.bind({ id: '#Rtn-Store', data: store, value: 'Id', text: 'Description' });

//                Modal.open({ id: '#modalRtn', title: title, action: action, obj: obj });
//                Table.add({ id: '#tableRtnItem', data: obj.VoucherItem, selectPick: true });
//                Rtn.sumOfTotalRtnItem();

//                //setTimeout(() => {
//                //    OnlineApi.pinCode({
//                //        postOfficeId: '#Rtn-ConPostOffice',
//                //        stateName: '#Rtn-ConStateName'
//                //    });
//                //}, 100);
//            }
//        });
//    }

//    static update(obj) {
//        Data.update({
//            url: 'Rtn/Update',
//            data: obj,
//            onSuccess: (response) => {
//                Message.show(response);
//                if (response.status == Message.Type.success) {
//                    Modal.close({ id: '#modalRtn' });
//                    Table.updateById({ id: '#tableRtn', objId: response.obj.Id, obj: response.obj });
//                }
//            }
//        });
//    }

//    static delete({ id }) {
//        Message.confirm({
//            msg: "Do you want to delete???",
//            confirmButtonText: "Delete",
//            denyButtonText: "Don't Delete",
//            data: id,
//            onConfirm: (id) => {
//                Data.delete({
//                    url: `Rtn/Delete?Id=${id}`,
//                    onSuccess: Rtn.deleteOnSuccess
//                });
//            }
//        });
//    }

//    static deleteOnSuccess = (response) => {
//        Message.show(response);
//        if (response.status == Message.Type.success) {
//            Table.updateById({ id: "#tableRtn", objId: response.obj.Id, obj: response.obj });
//        }
//    }
//}

//window.tableRtnRefNoAndDate = (value, obj, index) => {
//    return `
//        <div>${obj.RefNo ?? ''}</div>
//        <div>${obj.RefDate ? moment(obj.RefDate).format('DD-MMM-YYYY') : ''}</div>
//    `;
//};

//window.tableRtnSlNo = (value, obj, index) => {
//    return index + 1;
//}

//window.tableRtnDate = (value, obj, index) => {
//    return moment(obj.Date).format('DD-MMM-YYYY');
//}

//window.tableRtnStatus = (value, obj, index) => {
//    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
//}

//window.tableRtnCreatedByAndAt = (value, obj, index) => {
//    return `
//        <div>${obj.CreatedByName}</div>
//        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
//    `;
//}

//window.tableRtnUpdatedByAndAt = (value, obj, index) => {
//    return `
//        <div>${obj.UpdatedByName}</div>
//        <div>${DateTime.dateTime(obj.UpdatedAt)}</div>
//    `;
//}

//window.tableRtnAction = (value, obj, index) => {
//    let actionBtn = `
//        <div class="btn-group dropstart">            
//            <button class="btn btn-sm border-0" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>
//            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
//                ${obj.IsEdit ?
//            `<li>
//                        <a href="#" class="dropdown-item text-success-100 btn-edit" title="View / Edit">
//                            <span class="fa fa-edit text-success-100"></span>&nbsp;&nbsp;View / Edit
//                        </a>
//                    </li>` : ``
//        }
//                ${obj.IsDuplicate ?
//            `<li>
//                        <a href="#" class="dropdown-item text-primary-100 btn-duplicate" title="Duplicate">
//                            <span class="fa fa-copy text-primary-100"></span>&nbsp;&nbsp;Duplicate
//                        </a>
//                    </li>` : ``
//        }
//                ${obj.IsDelete ?
//            `<li>
//                        <a href="#" class="dropdown-item text-danger-100 btn-delete" title="Delete">
//                            <span class="fa fa-trash text-danger-100"></span>&nbsp;&nbsp;Delete
//                        </a>
//                    </li>` : ``
//        }
//                ${obj.IsEnable ?
//            `<li>
//                        <a href="#" class="dropdown-item text-success-100 btn-enable" title="Enable">
//                            <span class="fa fa-toggle-on text-success-100"></span>&nbsp;&nbsp;Enable
//                        </a>
//                    </li>` : ``
//        }
//            </ul>
//        </div>
//    `;
//    return actionBtn;
//}

////window.tableRtnConAddress = (value, obj, index) => {
////    const parts = [
////        obj.ConAdd1,
////        obj.ConAdd2,
////        obj.ConPostOffice,
////        obj.ConPincode,
////        obj.ConStateName
////    ]
////        .filter(x => x && x.trim() !== "")
////        .map(x => x.trim());

////    if (parts.length === 0) return "";

////    let lines = [];
////    for (let i = 0; i < parts.length; i += 2) {
////        lines.push(parts.slice(i, i + 2).join(", "));
////    }

////    return lines.join("<br>");
////};

//window.tableRtnActionEvent = {
//    'click .btn-edit': (e, value, obj, index) => {
//        Rtn.edit({ id: obj.Id });
//    },
//    'click .btn-duplicate': (e, value, obj, index) => {
//        Rtn.edit({ id: obj.Id, action: 'Add' });
//    },
//    'click .btn-print': (e, value, obj, index) => {
//        Rtn.print({ id: obj.Id });
//    },
//    'click .btn-delete': (e, value, obj, index) => {
//        Rtn.delete({ id: obj.Id });
//    }
//}


//window.tableRtnItemSlNo = (value, obj, index) => {
//    return index + 1;
//}

//window.tableRtnItemDesc = (value, obj, index) => {
//    return `
//        ${Dropdown.html({ id: `RtnItem_${index}`, className: 'rtn-item', data: Rtn.item, value: 'Id', text: 'Description', initialValue: [obj.ItemId], json: true, parent: '.modal' })}
//        <input type="text" id="RtnItemRemarks_${index}" class="form-control form-control-sm mb-0 mt-1 rtn-item-remarks" maxlength="100" placeholder="Remarks" value="${obj.Remarks ?? ""}">
//    `;
//}

//window.tableRtnItemDescEvent = {
//    'change .rtn-item': (e, value, obj, index) => {
//        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
//        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });
//        obj.UnitDesc = itemJson?.UnitDesc ?? null;

//        obj.Rate = itemJson?.Rate ?? 0;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);

//        Table.updateByIndex({ id: '#tableRtnItem', index: index, obj: obj, value: obj.ItemId, event: e });
//        Rtn.sumOfTotalRtnItem();
//    },
//    'input .rtn-item-remarks': (e, value, obj, index) => {
//        obj.Remarks = e.currentTarget.value;
//        Table.updateByIndex({ id: '#tableRtnItem', index: index, obj: obj, value: obj.Remarks, event: e });
//    }
//}

//window.tableRtnItemRate = (value, obj, index) => {
//    return `
//        <input type="text" id="RtnItemRate_${index}" class="form-control form-control-sm text-right rtn-item-rate" value="${obj.Rate}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
//    `;
//}

//window.tableRtnItemRateEvent = {
//    'input .rtn-item-rate': (e, value, obj, index) => {
//        obj.Rate = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
//        Table.updateByIndex({ id: '#tableRtnItem', index: index, obj: obj, value: obj.Rate, event: e });
//        Rtn.sumOfTotalRtnItem();
//    }
//}

//window.tableRtnItemQty = (value, obj, index) => {
//    return `
//        <input type="text" id="RtnItemQty_${index}" class="form-control form-control-sm text-right rtn-item-qty" value="${obj.Qty}" oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
//    `;
//}

//window.tableRtnItemQtyEvent = {
//    'input .rtn-item-qty': (e, value, obj, index) => {
//        obj.Qty = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        obj.Amount = (obj.Rate * obj.Qty).toFixed(2);
//        Table.updateByIndex({ id: '#tableRtnItem', index: index, obj: obj, value: obj.Qty, event: e });
//        Rtn.sumOfTotalRtnItem();
//    }
//}

//window.tableRtnItemAmount = (value, obj, index) => {
//    return `
//        <input type="text" id="RtnItemAmt_${index}" class="form-control form-control-sm text-right rtn-item-amount" value="${obj.Amount}" oninput="this.value = _Number.validate({value: this.value, dp: 2, min: 0, max: 999999999999})">
//    `;
//}

//window.tableRtnItemAmountEvent = {
//    'input .rtn-item-amount': (e, value, obj, index) => {
//        obj.Amount = e.currentTarget.value == '' ? '0' : e.currentTarget.value;
//        Table.updateByIndex({ id: '#tableRtnItem', index: index, obj: obj, value: obj.Amount, event: e });
//        Rtn.sumOfTotalRtnItem();
//    }
//}

//window.tableRtnReasonCodeDesc = (value, obj, index) => {
//    return `
//        ${Dropdown.html({ id: `RtnReasonCode_${index}`, className: 'rtn-item', data: Rtn.reasonCode, value: 'Value', text: 'Description', initialValue: [obj.ReasonCode], json: true, parent: '.modal' })}
//    `;
//}

//window.tableRtnReasonCodeDescEvent = {
//    'change .rtn-item': (e, value, obj, index) => {
//        obj.ReasonCode = e.currentTarget.value || "";
//        Table.updateByIndex({
//            id: '#tableRtnItem',
//            index: index,
//            obj: obj,
//            value: obj.ReasonCode,
//            event: e
//        });
//    }
//};

//window.tableRtnStoreDesc = (value, obj, index) => {
//    return `
//        ${Dropdown.html({ id: `RtnStore_${index}`, className: 'rtn-item', data: Rtn.store, value: 'Id', text: 'Description', initialValue: [obj.StoreId], json: true, parent: '.modal' })}
//    `;
//}

//window.tableRtnStoreDescEvent = {
//    'change .rtn-item': (e, value, obj, index) => {
//        obj.StoreId = !Field.isNullOrEmpty(e.currentTarget.value) ? parseInt(e.currentTarget.value) : 0;
//        Table.updateByIndex({
//            id: '#tableRtnItem',
//            index: index,
//            obj: obj,
//            value: obj.StoreId,
//            event: e
//        });
//    }
//};

//window.tableTaskSerialNo = (value, obj, index) => {
//    return `<input type="text" id="RtnSerialNo_${index}" class="form-control form-control-sm mb-0 rtn-item" value="${obj.SerialNo}" maxlength="150" />`;
//}

//window.tableRtnSerialNoDescEvent = {
//    'input .rtn-item': (e, value, obj, index) => {
//        obj.SerialNo = e.currentTarget.value || "";
//        Table.updateByIndex({
//            id: '#tableRtnItem',
//            index: index,
//            obj: obj,
//            value: obj.SerialNo,
//            event: e
//        });
//    }
//};

//window.tableTaskBatchNo = (value, obj, index) => {
//    return `<input type="text" id="RtnBatchNo_${index}" class="form-control form-control-sm mb-0 rtn-item" value="${obj.BatchNo}" maxlength="150" />`;
//}

//window.tableRtnBatchNoDescEvent = {
//    'input .rtn-item': (e, value, obj, index) => {
//        obj.BatchNo = e.currentTarget.value || "";
//        Table.updateByIndex({
//            id: '#tableRtnItem',
//            index: index,
//            obj: obj,
//            value: obj.BatchNo,
//            event: e
//        });
//    }
//};

//window.tableRtnExpiryOn = (value, obj, index) => {
//    return `<input type="date" id="RtnExpiryOn_${index}" class="form-control form-control-sm mb-0 rtn-item" value="${obj.ExpiryOn ?? ""}"/>`;
//}

//window.tableRtnExpiryOnEvent = {
//    'input .rtn-item': (e, value, obj, index) => {
//        obj.ExpiryOn = e.currentTarget.value || "";
//        Table.updateByIndex({
//            id: '#tableRtnItem',
//            index: index,
//            obj: obj,
//            value: obj.ExpiryOn,
//            event: e
//        });
//    }
//};


class Rtn {

    static item = [];

    static init() {

        Rtn.getViewOption();

        $('#btnSearch').on('click', () => {
            Rtn.get({
                method: 'Get',
                onSuccess: (response) => {
                    Table.add({ id: '#tableRtn', data: response.data });
                }
            });
        });

        $('#btnExport').on('click', () => {
            Rtn.get({
                method: 'Export',
                onSuccess: (response) => {
                    Export.Base64ToExcel({ base64: response.base64, fileName: "RTN" });
                }
            });
        });

        $('#btnNewEntry').on('click', () => {
            Rtn.newEntry();
        });

        // ================= GDN CHANGE =================
        //$('#Rtn-GdnNoId').on('change', () => {

        //    let listGdnId = $('#Rtn-GdnNoId').val() || [];

        //    if (listGdnId.length === 0) {
        //        Table.add({ id: '#tableRtnItem', data: [] });
        //        return;
        //    }

        //    Message.confirm({
        //        msg: 'Do you want to load GDN items?',
        //        confirmButtonText: 'Yes',
        //        denyButtonText: 'No',
        //        data: listGdnId,
        //        onConfirm: (listGdnId) => {

        //            Data.post({
        //                url: 'Rtn/GetGdnItem',
        //                data: listGdnId,
        //                onSuccess: (response) => {
        //                    Table.add({
        //                        id: '#tableRtnItem',
        //                        data: response.data,
        //                        selectPick: true
        //                    });
        //                }
        //            });

        //        }
        //    });
        //});
        //$('#Rtn-GdnNoId').on('change', () => {

        //    let listGdnId = $('#Rtn-GdnNoId').val() || [];

        //    let manualItems = $('#tableRtnItem').bootstrapTable('getData')
        //        .filter(x => !x.SourceVoucherId || x.SourceVoucherId === null);


        //    if (listGdnId.length === 0) {
        //        Table.add({
        //            id: '#tableRtnItem',
        //            data: manualItems,
        //            selectPick: true
        //        });
        //        return;
        //    }

        //    Message.confirm({
        //        msg: 'Do you want to load DLN items?',
        //        confirmButtonText: 'Yes',
        //        denyButtonText: 'No',
        //        data: listGdnId,
        //        onConfirm: (listGdnId) => {

        //            Data.post({
        //                url: 'Rtn/GetGdnItem',
        //                data: listGdnId,
        //                onSuccess: (response) => {


        //                    let dlnItems = response.data.map(x => {
        //                        x.SourceVoucherId = x.SourceVoucherId || x.VoucherId;
        //                        return x;
        //                    });

        //                    let uniqueMap = {};

        //                    [...manualItems, ...dlnItems].forEach(x => {

        //                        let key = [
        //                            x.SourceVoucherId || 'MANUAL',
        //                            x.ItemId || 0,
        //                            x.SerialNo || '',
        //                            x.BatchNo || ''
        //                        ].join('_');

        //                        if (!uniqueMap[key]) {
        //                            uniqueMap[key] = x;
        //                        }
        //                    });

        //                    let finalData = Object.values(uniqueMap);

        //                    Table.add({
        //                        id: '#tableRtnItem',
        //                        data: finalData,
        //                        selectPick: true
        //                    });
        //                }
        //            });

        //        }
        //    });
        //});


        $('#Rtn-GdnNoId').on('change', () => {

            let listGdnId = $('#Rtn-GdnNoId').val() || [];

            let manualItems = $('#tableRtnItem').bootstrapTable('getData')
                .filter(x => !x.SourceVoucherId || x.SourceVoucherId === null && x.Id == 0);

               


            //if (listGdnId.length === 0) {
            //    Table.add({
            //        id: '#tableRtnItem',
            //        data: manualItems,
            //        selectPick: true
            //    });
            //    return;
            //}

            Message.confirm({
                msg: 'Do you want to load DLN items?',
                confirmButtonText: 'Yes',
                denyButtonText: 'No',
                data: listGdnId,
                onConfirm: (listGdnId) => {

                    Data.post({
                        url: 'Rtn/GetGdnItem',
                        data: listGdnId,
                        onSuccess: (response) => {


                            let dlnItems = response.data.map(x => {
                                x.SourceVoucherId = x.SourceVoucherId || x.VoucherId;
                                return x;
                            });

                            let uniqueMap = {};

                            [...manualItems, ...dlnItems].forEach(x => {

                                let key = [
                                    x.SourceVoucherId || 'MANUAL',
                                    x.ItemId || 0,
                                    x.SerialNo || '',
                                    x.BatchNo || ''
                                ].join('_');

                                if (!uniqueMap[key]) {
                                    uniqueMap[key] = x;
                                }
                            });

                            let finalData = Object.values(uniqueMap);

                            Table.add({
                                id: '#tableRtnItem',
                                data: finalData,
                                selectPick: true
                            });
                        }
                    });

                }
            });
        });




        $('#Rtn-BtnSave').on('click', () => {

            if (!Field.isMandatory({ class: '.required' })) return;

            let voucherItem = $('#tableRtnItem').bootstrapTable('getData')
                .filter(x => x.ItemId > 0 && x.Qty > 0);

            if (voucherItem.length === 0) {
                Message.error({ statusText: 'Add at least one RTN item' });
                return;
            }

            let obj = Data.serializeToObject({ formId: '#formRtn' });

            obj.VoucherItem = voucherItem;

            // 🔥 LINK RTN WITH GDN VOUCHERS
            obj.ListVoucherId = JSON.stringify($('#Rtn-GdnNoId').val());

            if (!obj.Id) {
                Rtn.add(obj);
            } else {
                Rtn.update(obj);
            }
        });
    }

    // ================= VIEW OPTIONS =================
    static getViewOption() {
        Data.get({
            url: 'Rtn/GetViewOption',
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#ListNo', data: response.data.Number, value: 'Id', text: 'No' });
                Dropdown.bind({ id: '#ListPartyId', data: response.data.Party, value: 'Id', text: 'Name' });
            }
        });
    }
    
    static getAddOption({ onSuccess }) {
        Data.get({ url: 'Rtn/GetAddOption', onSuccess: onSuccess });
    }
    static get({ method = 'Get', onSuccess }) {

        let obj = {
            ListStatus: $('#ListStatus').val(),
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListNo: $('#ListNo').val(),
            ListPartyId: $('#ListPartyId').val()
        };

        Data.post({ url: `Rtn/${method}`, data: obj, onSuccess: onSuccess });
    }

    static newEntry() {

        Rtn.getAddOption({
            onSuccess: (response) => {

                Rtn.item = response.data.Item || [];

                Dropdown.bind({ id: '#Rtn-Store', data: response.data.Store, value: 'Id', text: 'Description' });
                Dropdown.bind({ id: '#Rtn-GdnNoId', data: response.data.GdnNo, value: 'Id', text: 'No' });

                Modal.open({ id: '#modalRtn', title: 'RTN / Add', action: 'Add' });
            }
        });
    }

    static addRtnItem() {

        let obj = {
            Id: 0,
            VoucherId: 0,
            ItemId: 0,
            ItemDesc: null,
            SerialNo: null,
            BatchNo: null,
            Qty: 0,
            Rate: 0,
            Amount: 0,
            Remarks: null
        };

        Table.add({
            id: '#tableRtnItem',
            data: obj,
            action: 'append',
            selectPick: true
        });
    }
    static add(obj) {
        Data.post({
            url: 'Rtn/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.reset({ id: '#modalRtn' });
                    Table.add({ id: '#tableRtn', data: response.obj, action: 'prepend' });
                }
            }
        });
    }


    static edit({ id, action = 'Edit' }) {

        Data.get({
            url: `Rtn/Edit?Id=${id}`,
            onSuccess: (response) => {

                let option = response.data;
                let obj = response.obj;
                let storeId = null;
                if (obj.VoucherItem && obj.VoucherItem.length > 0) {
                    storeId = obj.VoucherItem[0].StoreId;
                }

                let gdnIds = [];
                if (obj.ListVoucherId) {
                    try {
                        gdnIds = JSON.parse(obj.ListVoucherId);  
                    } catch (e) {
                        console.error("Invalid ListVoucherId:", obj.ListVoucherId);
                    }
                }
                let title = action === 'Edit'
                    ? `RTN / Edit (No: ${obj.No})`
                    : `RTN / Add`;
                Modal.open({id: '#modalRtn',title: title,action: action,obj: obj});
                Dropdown.bind({id: '#Rtn-Store',data: option.Store || [],value: 'Id',text: 'Description',initialValue: storeId ? [storeId] : []});

                Dropdown.bind({id: '#Rtn-GdnNoId', data: option.GdnNo || [],value: 'Id', text: 'No',initialValue: gdnIds});

                Table.add({id: '#tableRtnItem', data: obj.VoucherItem || [],selectPick: true});
            }
        });
    }

        
    //static edit({ id, action = 'Edit' }) {
    //    Data.get({
    //        url: `Rtn/Edit?Id=${id}`,
    //        onSuccess: (response) => {

    //            let option = response.data;
    //            Rtn.item = option.Item || [];

    //            let obj = response.obj;
    //            let title = action == 'Edit' ? `RTN / Edit (No: ${obj.No})` : `RTN / Add`;

    //           // Dropdown.bind({ id: '#Rtn-Store', data: response.data.Store, value: 'Id', text: 'Description', initialValue: obj.voucherItem.StoreId });
    //            //Dropdown.bind({ id:'#Rtn-GdnNoId', data: response.data.GdnNo, value: 'Id', text: 'No' });

    //            Dropdown.bind({
    //                id: '#Rtn-Store',
    //                data: response.data.Store,
    //                value: 'Id',
    //                text: 'Description',
    //                initialValue: [obj.StoreId]   // ✅ correct
    //            });

    //            let gdnIds = [];
    //            if (obj.ListVoucherId) {
    //                try {
    //                    gdnIds = JSON.parse(obj.ListVoucherId);
    //                } catch (e) {
    //                    console.error("Invalid ListVoucherId JSON:", obj.ListVoucherId);
    //                }
    //            }

    //            Dropdown.bind({
    //                id: '#Rtn-GdnNoId',
    //                data: response.data.GdnNo,
    //                value: 'Id',
    //                text: 'No',
    //                initialValue: gdnIds          // ✅ correct
    //            });


    //            Modal.open({ id: '#modalRtn', title: title, action: action, obj: obj });
    //            Table.add({ id: '#tableRtnItem', data: obj.VoucherItem, selectPick: true });
    //        }
    //    });
    //}
    static update(obj) {
        Data.update({
            url: 'Rtn/Update',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    Modal.close({ id: '#modalRtn' });
                    Table.updateById({ id: '#tableRtn', objId: response.obj.Id, obj: response.obj });
                }
            }
        });
    }
}


/* ====================== MAIN TABLE FORMATTERS ====================== */

window.tableRtnSlNo = (value, obj, index) => index + 1;

window.tableRtnDate = (value, obj, index) => {
    return obj.Date ? moment(obj.Date).format('DD-MMM-YYYY') : '';
};

window.tableRtnStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
};

window.tableRtnCreatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.CreatedByName ?? ''}</div>
        <div>${moment(obj.CreatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
};

window.tableRtnUpdatedByAndAt = (value, obj, index) => {
    return `
        <div>${obj.UpdatedByName ?? ''}</div>
        <div>${moment(obj.UpdatedAt).format('DD-MMM-YYYY HH:mm')}</div>
    `;
};

window.tableRtnAction = (value, obj, index) => {
    let actionBtn = [];

    if (obj.IsEdit) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-success-100 btn-edit">
                    <span class="fa fa-edit"></span>&nbsp;&nbsp;View / Edit
                </a>
            </li>
        `);
    }

    if (obj.IsDelete) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-danger-100 btn-delete">
                    <span class="fa fa-trash"></span>&nbsp;&nbsp;Delete
                </a>
            </li>
        `);
    }

    return `
        <div class="btn-group dropstart">
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${actionBtn.join('')}
            </ul>
        </div>
    `;
};

window.tableRtnActionEvent = {
    //'click .btn-edit': (e, value, obj, index) => {
    //    Rtn.edit({ id: obj.Id });
    //},
    'click .btn-edit': function (e, value, row, index) {
        e.preventDefault();
        Rtn.edit({ id: row.Id });
    },
    'click .btn-delete': (e, value, obj, index) => {
        Rtn.delete({ id: obj.Id });
    }
};


/* ====================== ITEM TABLE FORMATTERS ====================== */

window.tableRtnItemSlNo = (value, obj, index) => index + 1;

/* ---------- ITEM DESCRIPTION ---------- */
window.tableRtnItemDesc = (value, obj, index) => {
    return `
        ${Dropdown.html({
        id: `RtnItem_${index}`,
        className: 'rtn-item',
        data: Rtn.item,
        value: 'Id',
        text: 'Description',
        initialValue: [obj.ItemId],
        json: true,
        parent: '.modal'
    })}
    `;
};

window.tableRtnItemDescEvent = {
    'change .rtn-item': (e, value, obj, index) => {

        obj.ItemId = !Field.isNullOrEmpty(e.currentTarget.value)
            ? parseInt(e.currentTarget.value)
            : 0;

        let itemJson = Dropdown.itemJson({ id: `#${e.currentTarget.id}` });

        obj.ItemDesc = itemJson?.Description ?? null;
        obj.Rate = itemJson?.Rate ?? 0;

        obj.Amount = (parseFloat(obj.Rate) * parseFloat(obj.Qty || 0)).toFixed(2);

        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.ItemId,
            event: e
        });
    }
};


/* ---------- SERIAL NO ---------- */
window.tableTaskSerialNo = (value, obj, index) => {
    return `
        <input type="text"
               id="RtnSerialNo_${index}"
               class="form-control form-control-sm rtn-item"
               value="${obj.SerialNo ?? ''}"
               maxlength="150" />
    `;
};

window.tableRtnSerialNoDescEvent = {
    'input .rtn-item': (e, value, obj, index) => {
        obj.SerialNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.SerialNo,
            event: e
        });
    }
};


/* ---------- BATCH NO ---------- */
window.tableTaskBatchNo = (value, obj, index) => {
    return `
        <input type="text"
               id="RtnBatchNo_${index}"
               class="form-control form-control-sm rtn-item"
               value="${obj.BatchNo ?? ''}"
               maxlength="150" />
    `;
};

window.tableRtnBatchNoDescEvent = {
    'input .rtn-item': (e, value, obj, index) => {
        obj.BatchNo = e.currentTarget.value || "";
        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.BatchNo,
            event: e
        });
    }
};


/* ---------- QTY ---------- */
window.tableRtnItemQty = (value, obj, index) => {
    return `
        <input type="text"
               id="RtnItemQty_${index}"
               class="form-control form-control-sm text-right rtn-item-qty"
               value="${obj.Qty}"
               oninput="this.value = _Number.validate({value: this.value, dp: 3, min: 0, max: 999999})">
    `;
};

window.tableRtnItemQtyEvent = {
    'input .rtn-item-qty': (e, value, obj, index) => {

        obj.Qty = e.currentTarget.value === '' ? '0' : e.currentTarget.value;
        obj.Amount = (parseFloat(obj.Rate || 0) * parseFloat(obj.Qty || 0)).toFixed(2);

        Table.updateByIndex({
            id: '#tableRtnItem',
            index: index,
            obj: obj,
            value: obj.Qty,
            event: e
        });
    }
};


/* ---------- AMOUNT (OPTIONAL DISPLAY) ---------- */
window.tableRtnItemAmount = (value, obj, index) => {
    return `
        <input type="text"
               id="RtnItemAmt_${index}"
               class="form-control form-control-sm text-right rtn-item-amount"
               value="${obj.Amount}"
               readonly>
    `;
};


/* ====================== DELETE HANDLER ====================== */
Rtn.delete = ({ id }) => {
    Message.confirm({
        msg: "Do you want to delete this RTN?",
        confirmButtonText: "Delete",
        denyButtonText: "Cancel",
        data: id,
        onConfirm: (id) => {
            Data.delete({
                url: `Rtn/Delete?Id=${id}`,
                onSuccess: (response) => {
                    Message.show(response);
                    if (response.status === Message.Type.success) {
                        Table.updateById({
                            id: "#tableRtn",
                            objId: response.obj.Id,
                            obj: response.obj
                        });
                    }
                }
            });
        }
    });
};
