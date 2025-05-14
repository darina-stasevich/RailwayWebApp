import React from 'react';
import styles from './ScheduleModal.module.css';

const ScheduleModal = ({ isOpen, onClose, scheduleData, isLoading, error, formatDateTime }) => {
    if (!isOpen) {
        return null;
    }

    const formatDateOnly = (dateOnlyString) => {
        if (!dateOnlyString) return 'N/A';
        try {
            const [year, month, day] = dateOnlyString.split('-');
            const date = new Date(Date.UTC(year, month - 1, day));
            return new Intl.DateTimeFormat('ru-RU', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                timeZone: 'UTC'
            }).format(date);
        } catch (e) {
            console.error("Ошибка форматирования DateOnly:", e, dateOnlyString);
            return dateOnlyString;
        }
    };


    return (
        <div className={styles.modalOverlay}>
            <div className={styles.modalContent}>
                <button onClick={onClose} className={styles.closeButton}>×</button>
                <h2>Расписание маршрута</h2>
                {isLoading && <p>Загрузка расписания...</p>}
                {error && <p className={styles.errorMessage}>{error}</p>}
                {scheduleData && !isLoading && !error && (
                    <div>
                        <p><strong>Номер поезда:</strong> {scheduleData.trainNumber}</p>
                        <p><strong>Дата отправления:</strong> {formatDateOnly(scheduleData.departureDate)}</p>
                        {scheduleData.segments.map((segment, index) => (
                            <div key={index} className={styles.segmentCard}>
                                <p className={styles.segmentHeader}>
                                    <strong>{segment.fromStation} → {segment.toStation}</strong>
                                </p>
                                <div className={styles.segmentTimesGrid}>
                                    <span className={styles.timeLabel}>Отправление</span>
                                    <span className={styles.timeLabel}>Прибытие</span>
                                    <span className={styles.timeValue}>{formatDateTime(segment.departureDate)}</span>
                                    <span className={styles.timeValue}>{formatDateTime(segment.arrivalDate)}</span>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
                {!scheduleData && !isLoading && !error && <p>Нет данных для отображения.</p>}
            </div>
        </div>
    );
};

export default ScheduleModal;