import styles from './DirectRouteSegmentCard.module.css'
const DirectRouteSegmentCard = ({ directRoute, formatDuration, formatDateTime, getStationNameById, onShowSchedule}) => {
    return (
        <div className={styles.directRouteCard}>
            <div className={styles.twoColumnLayout}>
                <div className={styles.infoColumn}>
                    <p><strong>{getStationNameById(directRoute.fromStationId)} → {getStationNameById(directRoute.toStationId)}</strong></p>
                    <p>Отправление: {formatDateTime(directRoute.departureDate)}</p>
                    <p>Прибытие: {formatDateTime(directRoute.arrivalDate)}</p>
                    <p>Время в пути: {formatDuration(directRoute.timeInTransit)}</p>
                    <p>Стоимость: от {directRoute.minimalCost?.toFixed(2)} до {directRoute.maximumCost?.toFixed(2)} руб.</p>
                    <p>Свободных мест: {directRoute.availableSeats}</p>
                </div>
                <div className={styles.actionsColumn}>
                    <button
                        onClick={() => onShowSchedule(directRoute.concreteRouteId)}
                        className={styles.scheduleButton}>
                        Посмотреть расписание
                    </button>
                </div>
            </div>
        </div>
    );
};
export default DirectRouteSegmentCard;