import React, { useState, useEffect } from 'react';
import styles from './TicketBookingPage.module.css';
import { useLocation, useNavigate } from 'react-router-dom';
import {formatDateTime, formatDuration} from "../../utils/formatters.js";

const TicketBookingPage = () => {

    const location = useLocation();
    const navigate = useNavigate();
    const selectedComplexRoute = location.state?.selectedComplexRoute;
    const fromStation = location.state?.fromStation;
    const toStation = location.state?.toStation;


    useEffect(() => {
        if (!selectedComplexRoute) {
            console.warn("Данные о маршруте не были переданы. Возврат на главную.");
             navigate('/find-route');
        }
    }, [selectedComplexRoute, navigate]);

    if (!selectedComplexRoute) {
        return <p>Загрузка данных о маршруте или перенаправление...</p>;
    }

    console.log("Получен маршрут на странице бронирования:", selectedComplexRoute);

    return (
        <div className={styles.ticketBookingPageContainer}>
            <h1>Бронирование билетов</h1>
            <h2>{fromStation} → {toStation}</h2>
            <p>{formatDateTime(selectedComplexRoute.departureDate)} - {formatDateTime(selectedComplexRoute.arrivalDate)}</p>
            <p>Общее время в пути: {formatDuration(selectedComplexRoute.totalDuration)}</p>

            <h3>Участки маршрута для бронирования:</h3>
            {selectedComplexRoute.directRoutes.map((segment, index) => (
                <div key={segment.concreteRouteId || index} className={styles.segmentBookingCard}>
                    <h4>Сегмент {index + 1}: {/* Добавь названия станций, если они есть в segment */} </h4>
                    <p>ID конкретного маршрута (для запроса вагонов): {segment.concreteRouteId}</p>
                    <p>StartSegmentNumber: {segment.startSegmentNumber}, EndSegmentNumber: {segment.endSegmentNumber}</p>
                    {/*
                        Здесь будет UI для выбора вагона (запрос ShortCarriageInfoDto)
                        и затем выбора места (запрос DetailedCarriageInfoDto) для этого сегмента.
                    */}
                    <button type="button">Выбрать вагон для этого участка</button>
                </div>
            ))}
            {/* Здесь будет общая сумма и кнопка "Оформить заказ" */}
        </div>
    );
}

export default TicketBookingPage;

