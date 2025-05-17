export const getLocalDateInputString = (utcDateString) => {
    if (!utcDateString) return '';
    const dateObj = new Date(utcDateString);

    if (isNaN(dateObj.getTime())) {
        console.error("Невалидная строка UTC даты получена:", utcDateString);
        return '';
    }

    const year = dateObj.getFullYear();
    const month = (dateObj.getMonth() + 1).toString().padStart(2, '0');
    const day = dateObj.getDate().toString().padStart(2, '0');

    return `${year}-${month}-${day}`;
};

export const mapServerGenderToSelectValue = (serverGender) => {
    if (serverGender === 0 || serverGender === '0' || String(serverGender).toLowerCase() === 'male') {
        return 0;
    } else if (serverGender === 1 || serverGender === '1' || String(serverGender).toLowerCase() === 'female') {
        return 1;
    }
    return '';
};
