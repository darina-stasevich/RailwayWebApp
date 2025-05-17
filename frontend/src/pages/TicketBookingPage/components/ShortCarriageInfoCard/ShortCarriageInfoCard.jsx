import React from 'react';
import styles from './ShortCarriageInfoCard.module.css';
import buttonStyles from '../../../../styles/ButtonStyles.module.css';
const ShortCarriageInfoCard = ({ carriageSummary, selectedInThisCarriageCount, onViewDetails, isLoadingDetails }) => {
    return (
        <div className={styles.shortCarriageItem}>
            <p>Вагон №{carriageSummary.carriageNumber} ({carriageSummary.layoutIdentifier}), мест: {carriageSummary.availableSeats}, цена от: {carriageSummary.cost.toFixed(2)} руб.</p>
            {selectedInThisCarriageCount > 0 && <p><small>Выбрано в этом вагоне: {selectedInThisCarriageCount}</small></p>}
            <button className={buttonStyles.showSeatsButton}
                    onClick={onViewDetails} disabled={isLoadingDetails}>
                    {isLoadingDetails ? "Загрузка..." : "Выбрать/посмотреть места"}
            </button>
        </div>
    );
};

export default ShortCarriageInfoCard;