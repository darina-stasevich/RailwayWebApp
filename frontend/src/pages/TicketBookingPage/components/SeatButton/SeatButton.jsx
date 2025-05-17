import React from 'react';
import styles from '../../../../styles/ButtonStyles.module.css';

const SeatButton = ({ seatNum, isAvailable, isSelected, onClick, title }) => {
    return (
        <button
            className={`${styles.seatButton} ${isSelected ? styles.seatSelected : ''} ${!isAvailable ? styles.seatUnavailable : ''}`}
            onClick={onClick}
            disabled={!isAvailable}
            title={title || (!isAvailable ? "Место занято" : `Место ${seatNum}`)}
        >
            {seatNum}
        </button>
    );
};

export default SeatButton;