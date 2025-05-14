import styles from './FindRoutePage.module.css'
import DirectRouteSegmentCard from './DirectRouteSegmentCard.jsx'
import React from "react";

const ComplexRouteCard = ({ routeIndex, complexRoute, formatDuration, formatDateTime, getStationNameById, onShowSchedule}) => {
    return (
        <div key={routeIndex + 1} className={styles.complexRouteCard}>
            <h3>Маршрут {routeIndex + 1}</h3>
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