import React from 'react';
import styles from './SeatCard.module.css';

const SeatCard = ({seat, getStationNameById, formatDateTime, formatGender, formatDateOnly }) => {
    const passengerData = seat.passengerData || {};

    return (
        <div className={styles.seatCard}>
            <h3>Вагон {seat.carriage}, Место {seat.seatNumber}</h3>
            <div>
                <div className={styles.infoDetailItem}>
                    <span className={styles.infoLabel}><strong>Маршрут:</strong></span>
                    <span className={styles.infoValue}>{getStationNameById(seat.fromStationId)} → {getStationNameById(seat.toStationId)}</span>
                </div>
                <div className={styles.infoDetailItem}>
                    <span className={styles.infoLabel}><strong>Отправление:</strong></span>
                    <span className={styles.infoValue}>{formatDateTime(seat.departureDate)}</span>
                </div>
                <div className={styles.infoDetailItem}>
                    <span className={styles.infoLabel}><strong>Прибытие:</strong></span>
                    <span className={styles.infoValue}>{formatDateTime(seat.arrivalDate)}</span>
                </div>
                <div className={styles.infoDetailItem}>
                    <span className={styles.infoLabel}><strong>Цена:</strong></span>
                    <span className={styles.infoValue}>{seat.price ? seat.price.toFixed(2) : 'N/A'} бел. руб.</span>
                </div>
                <div className={styles.infoDetailItem}>
                    <span className={styles.infoLabel}><strong>Постельное бельё:</strong></span>
                    <span className={styles.infoValue}>{seat.hasBedLinenSet ? 'Да' : 'Нет'}</span>
                </div>
            </div>

            <div className={styles.passengerInfo}>
                <h4>Пассажир:</h4>
                <div className={styles.infoDetailItem}>
                    <span className={styles.infoLabel}><strong>Пол:</strong></span>
                    <span className={styles.infoValue}>{formatGender(passengerData.gender)}</span>
                </div>
                <div className={styles.infoDetailItem}>
                    <span className={styles.infoLabel}><strong>Дата рождения:</strong></span>
                    <span className={styles.infoValue}>{passengerData.birthDate}</span>
                </div>
                <div className={styles.infoDetailItem}>
                    <span className={styles.infoLabel}><strong>Документ:</strong></span>
                    <span className={styles.infoValue}>{passengerData.passportNumber}</span>
                </div>
            </div>
        </div>
    );
};

export default SeatCard;