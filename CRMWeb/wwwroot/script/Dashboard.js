class Dashboard {
    static init() {
        Dashboard.getViewOption((response) => {
            if (!response || !response.data) return;

            const d = response.data;

            $('#passTotal').text(d.PassTotal ?? 0);
            $('#medicalTotal').text(d.MedicalTotal ?? 0);
            $('#trainingTotal').text(d.TrainingTotal ?? 0);
            $('#labourTotal').text(d.LabourTotal ?? 0);

            $('#passExpirySoon').text(d.PassExpirySoon ?? 0);
            $('#medicalExpirySoon').text(d.MedicalExpirySoon ?? 0);
            $('#trainingExpirySoon').text(d.TrainingExpirySoon ?? 0);
            $('#labourExpirySoon').text(d.LabourExpirySoon ?? 0);

            $('#passExpired').text(d.PassExpired ?? 0);
            $('#medicalExpired').text(d.MedicalExpired ?? 0);
            $('#trainingExpired').text(d.TrainingExpired ?? 0);
            $('#labourExpired').text(d.LabourExpired ?? 0);

            $('#receiptNoteTotal').text(d.ReceiptNoteTotal ?? 0);
            $('#receiptNoteMonth').text(d.ReceiptNoteMonth ?? 0);
            $('#receiptNoteToday').text(d.ReceiptNoteToday ?? 0);

            // Stock In
            $('#stockInTotal').text(d.StockInTotal ?? 0);
            $('#stockInMonth').text(d.StockInMonth ?? 0);
            $('#stockInToday').text(d.StockInToday ?? 0);

            // Delivery Note
            $('#deliveryNoteTotal').text(d.DeliveryNoteTotal ?? 0);
            $('#deliveryNoteMonth').text(d.DeliveryNoteMonth ?? 0);
            $('#deliveryNoteToday').text(d.DeliveryNoteToday ?? 0);

            // Return Note
            $('#returnNoteTotal').text(d.ReturnNoteTotal ?? 0);
            $('#returnNoteMonth').text(d.ReturnNoteMonth ?? 0);
            $('#returnNoteToday').text(d.ReturnNoteToday ?? 0);

            // Stock Out
            $('#stockOutTotal').text(d.StockOutTotal ?? 0);
            $('#stockOutMonth').text(d.StockOutMonth ?? 0);
            $('#stockOutToday').text(d.StockOutToday ?? 0);

            // Stock Transfer
            $('#stockTransferTotal').text(d.StockTransferTotal ?? 0);
            $('#stockTransferMonth').text(d.StockTransferMonth ?? 0);
            $('#stockTransferToday').text(d.StockTransferToday ?? 0);

            // Stock Adjustment
            $('#stockAdjustmentTotal').text(d.StockAdjustmentTotal ?? 0);
            $('#stockAdjustmentMonth').text(d.StockAdjustmentMonth ?? 0);
            $('#stockAdjustmentToday').text(d.StockAdjustmentToday ?? 0);
        });
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Dashboard/GetViewOption', onSuccess: onSuccess });
    }
}