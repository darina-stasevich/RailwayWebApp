import React from 'react';
import styles from './SegmentCard.module.css';

const SegmentCard = ({ segment, index, bookingForThisSegment, isActiveForBooking, getStationNameById, formatDateTime, children }) => {
    let totalSelectedSeatsForSegment = 0;
    if (bookingForThisSegment?.selectionsByCarriage) {
        bookingForThisSegment.selectionsByCarriage.forEach(setOfSeats => {
            totalSelectedSeatsForSegment += setOfSeats.size;
        });
    }

    return (
        <div className={`${styles.segmentBookingCard} ${isActiveForBooking ? styles.activeSegment : ''} ${bookingForThisSegment?.isConfigurationDone ? styles.completedSegment : ''}`}>
            <h4>Участок {index + 1}: {getStationNameById(segment.fromStationId)} - {getStationNameById(segment.toStationId)}
                {bookingForThisSegment?.isConfigurationDone && <span> (Выбрано мест: {totalSelectedSeatsForSegment})</span>}
            </h4>
            <p>Отправление: {formatDateTime(segment.departureDate)}</p>
            <p>Прибытие:    {formatDateTime(segment.arrivalDate)}</p>
            {isActiveForBooking && children}
        </div>
    );
};

export default SegmentCard;