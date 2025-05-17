import React from 'react';
import SeatButton from '../SeatButton/SeatButton.jsx';
import styles from './DetailedCarriageInfoCard.module.css';
import buttonStyles from '../../../../styles/ButtonStyles.module.css'
const DetailedCarriageInfoCard = ({ carriageDetails, temporarySeats, onToggleSeat, onSave, onBackToList }) => {
    if (!carriageDetails) return null;

    return (
        <div className={styles.detailedCarriageView}>
            <h5>Вагон №{carriageDetails.carriageNumber} ({carriageDetails.layoutIdentifier})</h5>
            <p>Всего мест: {carriageDetails.totalSeats}, доступно: {carriageDetails.availableSeats.length}, цена: {carriageDetails.cost.toFixed(2)}</p>
            <div className={styles.seatSchema}>
                {Array.from({ length: carriageDetails.totalSeats }, (_, i) => i + 1).map(seatNum => {
                    const isAvailable = carriageDetails.availableSeats.includes(seatNum);
                    const isSelectedTemporarily = temporarySeats.has(seatNum);
                    return (
                        <SeatButton
                            key={seatNum}
                            seatNum={seatNum}
                            isAvailable={isAvailable}
                            isSelected={isSelectedTemporarily}
                            onClick={() => isAvailable && onToggleSeat(seatNum)}
                        />
                    );
                })}
            </div>
            <button className={buttonStyles.saveSeatsButton}
                    onClick={onSave}>Сохранить выбор для этого вагона
            </button>
            <button className={buttonStyles.backButton}
                    onClick={onBackToList}>Назад к списку вагонов
            </button>
        </div>
    );
};

export default DetailedCarriageInfoCard;