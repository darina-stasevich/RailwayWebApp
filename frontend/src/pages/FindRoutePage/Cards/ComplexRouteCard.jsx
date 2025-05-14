import styles from './ComplexRouteCard.module.css'
import DirectRouteSegmentCard from './DirectRouteSegmentCard.jsx'
import React from "react";
import { useNavigate } from 'react-router-dom';

const ComplexRouteCard = ({ routeIndex, complexRoute, formatDuration, formatDateTime, getStationNameById, onShowSchedule}) => {
    const navigate = useNavigate();

    const handleBookTicketsClick = () => {

        let fromStationName = 'Неизвестно';
        let toStationName = 'Неизвестно';

        if (complexRoute && complexRoute.directRoutes && complexRoute.directRoutes.length > 0) {
            const firstSegment = complexRoute.directRoutes[0];
            if (firstSegment && firstSegment.fromStationId) {
                fromStationName = getStationNameById(firstSegment.fromStationId);
            }

            const lastSegment = complexRoute.directRoutes[complexRoute.directRoutes.length - 1];
            if (lastSegment && lastSegment.toStationId) {
                toStationName = getStationNameById(lastSegment.toStationId);
            }
        }

        navigate('/tickets-booking', { state: {
            selectedComplexRoute: complexRoute,
            fromStation: fromStationName,
            toStation: toStationName} });
    };

    return (
        <div key={routeIndex + 1} className={styles.complexRouteCard}>
            <div className={styles.complexRouteHeader}>
                <h3 className={styles.complexRouteTitle}>Маршрут {routeIndex + 1}</h3>
                <button
                    onClick={handleBookTicketsClick}
                    type="button"
                    className={styles.bookTicketsButton}
                >
                    Заказать билеты
                </button>
            </div>
            <p><strong>Время в пути:</strong> {formatDuration(complexRoute.totalDuration)}</p>
            <p><strong>Отправление:</strong> {formatDateTime(complexRoute.departureDate)}</p>
            <p><strong>Прибытие:</strong> {formatDateTime(complexRoute.arrivalDate)}</p>
            <p><strong>Стоимость:</strong> от {complexRoute.minimalTotalCost?.toFixed(2)} до {complexRoute.maximumTotalCost?.toFixed(2)} руб.</p>

            <h4>Участки маршрута:</h4>
            {complexRoute.directRoutes.map((directRoute, drIndex) => (
                <DirectRouteSegmentCard
                    key={directRoute.concreteRouteId || drIndex}
                    directRoute={directRoute}
                    formatDuration={formatDuration}
                    formatDateTime={formatDateTime}
                    getStationNameById={getStationNameById}
                    onShowSchedule={onShowSchedule}>
                </DirectRouteSegmentCard>
            ))}

        </div>
        );
};
export default ComplexRouteCard;