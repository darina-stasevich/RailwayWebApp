import React from "react";
import styles from './BookingCard.module.css';
import buttonStyles from '../../../styles/ButtonStyles.module.css';
import SeatCard from "./SeatCard";

const BookingCard = ({booking, getStationNameById, formatGender, formatDateOnly, handlePayBooking, formatDateTime}) => {
    return (<div key={booking.seatLockId} className={styles.bookingCard}>
        <div className={styles.bookingHeader}>
            <h2>Бронирование #{booking.seatLockId.substring(0, 8)}</h2>
            <p>Действительно до: {formatDateTime(booking.expirationTimeUtc)}</p>
        </div>

        <div className={styles.seatsList}>
            {booking.lockedSeatInfos.map((seat, index) => (
                <SeatCard
                    key={`${booking.seatLockId}-${seat.carriage}-${seat.seatNumber}-${index}`}
                    formatDateTime={formatDateTime}
                    formatDateOnly={formatDateOnly}
                    formatGender={formatGender}
                    getStationNameById={getStationNameById}
                    seat={seat}
                />
            ))}
        </div>
        <button
            onClick={() => handlePayBooking(booking.seatLockId)}
            className={buttonStyles.confirmButton}
        >
            Оплатить бронирование
        </button>
    </div>
    );
}

export default BookingCard;