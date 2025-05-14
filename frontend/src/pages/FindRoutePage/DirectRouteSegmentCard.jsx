import styles from './FindRoutePage.module.css'
const DirectRouteSegmentCard = ({ directRoute, formatDuration, formatDateTime, getStationNameById }) => {
    return (
        <div className={styles.directRouteCard}>
            <p>Откуда: {getStationNameById(directRoute.fromStationId)}</p>
            <p>Куда: {getStationNameById(directRoute.toStationId)}</p>
            <p>Отправление: {formatDateTime(directRoute.departureDate)}</p>
            <p>Прибытие: {formatDateTime(directRoute.arrivalDate)}</p>
            <p>Время в пути: {formatDuration(directRoute.timeInTransit)}</p>
            <p>Стоимость: от {directRoute.minimalCost?.toFixed(2)} до {directRoute.maximumCost?.toFixed(2)} руб.</p>
            <p>Свободных мест: {directRoute.availableSeats}</p>
        </div>
    );
};
export default DirectRouteSegmentCard;