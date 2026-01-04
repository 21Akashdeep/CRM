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
        });
    }
    static getViewOption(onSuccess = () => { }) {
        Data.get({ url: 'Dashboard/GetViewOption', onSuccess: onSuccess });
    }
}