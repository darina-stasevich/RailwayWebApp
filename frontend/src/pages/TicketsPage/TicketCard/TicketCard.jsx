import React from 'react';
import styles from './TicketCard.module.css';
import buttonStyles from '../../../styles/ButtonStyles.module.css';

const TicketCard = ({ ticket, getStationNameById, formatDateTime, formatGender, formatDateOnly, isCancellable, onCancelTicket }) => {
    if (!ticket) {
        return null;
    }

    const passengerData = ticket.passengerData || {};

    const handleCancelClick = () => {
        if (onCancelTicket && ticket && ticket.ticketId) {
            onCancelTicket(ticket.ticketId);
        }
    };

    return (
        <div className={styles.ticketCard}>
            <div className={styles.ticketHeader}>
                <h3 className={styles.routeTitle}>
                    {getStationNameById(ticket.fromStationId)} → {getStationNameById(ticket.toStationId)}
                </h3>
                <p className={styles.purchaseTime}>Куплен: {formatDateTime(ticket.purchaseTime)}</p>
            </div>

            <div className={styles.routeDetails}>
                <p><strong>Отправление:</strong> {formatDateTime(ticket.departureDate)}</p>
                <p><strong>Прибытие:</strong> {formatDateTime(ticket.arrivalDate)}</p>
                <p><strong>Поезд:</strong> {ticket.train}</p>
                <p><strong>Вагон:</strong> {ticket.carriage}</p>
                <p><strong>Место:</strong> {ticket.seat}</p>
                <p><strong>Цена:</strong> {ticket.price ? ticket.price.toFixed(2) : 'N/A'} бел. руб.</p>
                <p><strong>Постельное бельё:</strong> {ticket.hasBedLinenSet ? 'Да' : 'Нет'}</p>
            </div>

            {ticket.passengerData && (
                <div className={styles.passengerInfoSection}>
                    <h4 className={styles.passengerFullName}>
                        {passengerData.surname} {passengerData.firstName}
                        {passengerData.secondName && ` ${passengerData.secondName}`}
                    </h4>
                    <div className={styles.passengerDetailItem}>
                        <span className={styles.passengerLabel}><strong>Пол:</strong></span>
                        <span className={styles.passengerValue}>{formatGender(passengerData.gender)}</span>
                    </div>
                    <div className={styles.passengerDetailItem}>
                        <span className={styles.passengerLabel}><strong>Дата рождения:</strong></span>
                        <span className={styles.passengerValue}>{passengerData.birthDate}</span>
                    </div>
                    <div className={styles.passengerDetailItem}>
                        <span className={styles.passengerLabel}><strong>Документ:</strong></span>
                        <span className={styles.passengerValue}>{passengerData.passportNumber}</span>
                    </div>
                </div>
            )}

            {isCancellable && onCancelTicket && (
                <div className={styles.actionsSection}>
                    <button
                        onClick={handleCancelClick}
                        className={buttonStyles.cancelButton}
                    >
                        Отменить билет
                    </button>
                </div>
            )}
        </div>
    );
};

export default TicketCard;