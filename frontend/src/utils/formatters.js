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