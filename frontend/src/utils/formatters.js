export const formatDateTime = (dateTimeString) => {
    if (!dateTimeString) return 'N/A';
    try {
        const date = new Date(dateTimeString);
        return date.toLocaleString('ru-RU', {
            year: 'numeric', month: 'long', day: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    } catch (e) {
        return dateTimeString;
    }
};

export const formatDuration = (timespanString) => {
    if (!timespanString) return 'N/A';
    const parts = timespanString.split(':');
    let formatted = '';
    if (parts.length >= 3) {
        const hours = parseInt(parts[0].slice(-2), 10);
        const minutes = parseInt(parts[1], 10);
        if (hours > 0) formatted += `${hours} ч `;
        if (minutes > 0) formatted += `${minutes} мин`;
        if (!formatted) formatted = "Менее минуты";
    } else {
        return timespanString;
    }
    return formatted;
};

export const formatGender = (gender) => {
    if (gender === 0 || gender === '0') return 'Мужской';
    if (gender === 1 || gender === '1') return 'Женский';
    return gender !== null && gender !== undefined ? String(gender) : 'Не указан';
};

export const formatDateOnly = (utcDateTimeString) => {
    if (!utcDateTimeString) return 'N/A';
    try {
        const dateInUtc = new Date(utcDateTimeString);

        if (isNaN(dateInUtc.getTime())) {
            console.error("Invalid UTC DateTime string for formatDateOnly:", utcDateTimeString);
            return utcDateTimeString;
        }
        return new Intl.DateTimeFormat('ru-RU', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        }).format(dateInUtc);

    } catch (e) {
        console.error("Ошибка форматирования DateOnly из UTC DateTime:", e, utcDateTimeString);
        return utcDateTimeString;
    }
};