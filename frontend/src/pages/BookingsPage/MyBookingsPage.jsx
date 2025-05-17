import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate } from "react-router-dom";
import styles from './MyBookingsPage.module.css';
import { formatDateTime, formatDateOnly, formatGender } from "../../utils/formatters.js";
import { useStations } from '../../contexts/StationsContext.jsx';
import BookingCard from './Cards/BookingCard.jsx';

export const MyBookingsPage = () => {
    const navigate = useNavigate();

    const { stations, isLoadingStations, errorStations, getStationNameById } = useStations();

    const [books, setBooks] = useState([]);
    const [isLoadingBookings, setIsLoadingBookings] = useState(false);
    const [errorBookings, setErrorBookings] = useState('');

    const fetchBookings = useCallback(async () => {
        setIsLoadingBookings(true);
        setErrorBookings('');
        const token = localStorage.getItem('token');

        if (!token) {
            setErrorBookings('Ошибка: Пользователь не авторизован.');
            setIsLoadingBookings(false);
            navigate('/login');
            return;
        }

        try {
            const response = await fetch('http://localhost:5241/api/Books/my-bookings', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            if (!response.ok) {
                let errorMsg = `Ошибка загрузки бронирования: ${response.statusText}`;
                if (response.status === 401) {
                    errorMsg = "Сессия истекла или недействительна. Пожалуйста, войдите снова.";
                    navigate('/login');
                } else {
                    try {
                        const errorData = await response.json();
                        errorMsg = errorData.title || errorData.message || errorMsg;
                    } catch(e) {

                    }
                }
                throw new Error(errorMsg);
            }
            const data = await response.json();
            setBooks(data || []);
        } catch (err) {
            setErrorBookings(err.message);
            console.error("Ошибка при загрузке бронирований:", err);
        } finally {
            setIsLoadingBookings(false);
        }
    }, [navigate]);

    useEffect(() => {
        if (!isLoadingStations) {
            fetchBookings();
        }
    }, [fetchBookings, isLoadingStations]);

    const handlePayBooking = async (seatLockId) => {
        const token = localStorage.getItem('token');
        if (!token) {
            alert("Ошибка: Пользователь не авторизован.");
            navigate('/login');
            return;
        }

        try {
            const response = await fetch(`http://localhost:5241/api/Payments/pay`, {
                method: 'POST',
                body: JSON.stringify(seatLockId),
                headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: "Не удалось получить детали ошибки." }));
                throw new Error(`Ошибка оплаты ${response.status}: ${errorData.title || errorData.message || response.statusText}`);
            }
            await response.json();
            alert("Бронирование успешно оплачено! Билеты сформированы.");
            fetchBookings();
        } catch (paymentError) {
            console.error("Ошибка при оплате бронирования:", paymentError);
            alert(`Произошла ошибка при оплате: ${paymentError.message}`);
        } finally {
            setIsLoadingBookings(false);
        }
    };

    if (isLoadingStations) {
        return <div className={styles.loading}>Загрузка данных станций...</div>;
    }

    if (errorStations) {
        return <div className={styles.error}>Ошибка загрузки данных станций: {errorStations}. Пожалуйста, обновите страницу или попробуйте позже.</div>;
    }

    if (isLoadingBookings) {
        return <div className={styles.loading}>Загрузка ваших бронирований...</div>;
    }

    if (errorBookings) {
        return <div className={styles.error}>{errorBookings}</div>;
    }

    return (
        <div className={styles.container}>
            <h1>Мои бронирования</h1>

            {books.length === 0 ? (
                <div className={styles.noBookings}>Нет активных бронирований.</div>
            ) : (
                books.map((booking) => (
                    <BookingCard
                        key={booking.seatLockId}
                        booking = {booking}
                        getStationNameById = {getStationNameById}
                        formatGender = {formatGender}
                        formatDateOnly = {formatDateOnly}
                        handlePayBooking = {handlePayBooking}
                        formatDateTime={formatDateTime}
                    />
                ))
            )}
        </div>
    );
};

export default MyBookingsPage;