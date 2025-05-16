import React from 'react';
import styles from './RouteInfoDisplay.module.css'
const RouteInfoDisplay = ({ fromStation, toStation, selectedComplexRoute, formatDateTime, formatDuration }) => {
    if (!selectedComplexRoute) return null;

    return (
        <div className={styles.routeInfoSection}>
            <h2 className={styles.routeTitle}>{fromStation} → {toStation}</h2>
            <div className={styles.routeTimingInfo}>
                <span>{formatDateTime(selectedComplexRoute.departureDate)} - {formatDateTime(selectedComplexRoute.arrivalDate)}</span>
                <span className={styles.routeDuration}>Время в пути: {formatDuration(selectedComplexRoute.totalDuration)}</span>
            </div>
        </div>
    );
};

export default RouteInfoDisplay;