import React, { useState, useEffect } from 'react';
import styles from './FindRoutePage.module.css';
import { useNavigate } from 'react-router-dom';

const FindRoutePage = () => {
    const [stations, setStations] = useState([]);
    const [fromStationId, setFromStationId] = useState('');
    const [toStationId, setToStationId] = useState('');
    const [departureDate, setDepartureDate] = useState(() => {
        const today = new Date();
        return today.toISOString().split('T')[0];
    });
    const [isDirectRoute, setIsDirectRoute] = useState(true);
    const [searchResults, setSearchResults] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const fetchStations = async () => {
            setIsLoading(true);
            setError('');

            const token = localStorage.getItem('token');

            if (!token) {
                setError('Ошибка: Пользователь не авторизован. Не удалось загрузить станции.');
                setIsLoading(false);
                navigate('/login');
                return;
            }
            try {
                const response = await fetch('http://localhost:5241/api/Stations', {
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                });
                if (!response.ok) {
                    throw new Error(`Ошибка загрузки станций: ${response.statusText}`);
                }
                const data = await response.json();
                setStations(data || []);
            } catch (err) {
                setError(err.message);
                console.error("Ошибка при загрузке станций:", err);
            } finally {
                setIsLoading(false);
            }
        };

        fetchStations();
    }, []);

    const handleSearch = async (e) => {
        e.preventDefault();
        setError('');
        setSearchResults([]); // Очищаем предыдущие результаты

        if (!fromStationId || !toStationId || !departureDate) {
            setError('Пожалуйста, выберите станцию отправления, прибытия и дату.');
            return;
        }

        if (fromStationId === toStationId) {
            setError('Станция отправления и прибытия не могут совпадать.');
            return;
        }

        setIsLoading(true);
        const searchRequest = {
            fromStationId,
            toStationId,
            departureDate,
            isDirectRoute,
        };

        try {
            const response = await fetch('http://localhost:5241/api/Routes/search', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                },
                body: JSON.stringify(searchRequest),
            });

            if (!response.ok) {
                if (response.status === 400) {
                    const errorData = await response.json();
                    let messages = [];
                    if (errorData.errors) {
                        for (const key in errorData.errors) {
                            messages = messages.concat(errorData.errors[key]);
                        }
                    } else if (errorData.title) {
                        messages.push(errorData.title);
                    } else {
                        messages.push('Ошибка валидации на сервере.');
                    }
                    setError(messages.join(' '));
                } else {
                    throw new Error(`Ошибка поиска маршрутов: ${response.statusText}`);
                }
                setIsLoading(false);
                return;
            }

            const data = await response.json();
            setSearchResults(data || []);
            if (!data || data.length === 0) {
                setError('Маршруты по вашему запросу не найдены.');
            }
        } catch (err) {
            setError(err.message);
            console.error("Ошибка при поиске маршрутов:", err);
        } finally {
            setIsLoading(false);
        }
    };
    const formatDuration = (timespanString) => {
        if (!timespanString) return 'N/A';
        const parts = timespanString.split(':');
        let formatted = '';
        if (parts.length >= 3) {
            const hours = parseInt(parts[0].slice(-2), 10);
            const minutes = parseInt(parts[1], 10);
            if (hours > 0) formatted += `${hours} ч `;
            if (minutes > 0) formatted += `${minutes} мин`;
            if (!formatted) formatted = "Менее минуты";
        } else {
            return timespanString;
        }
        return formatted;
    };

    const formatDateTime = (dateTimeString) => {
        if (!dateTimeString) return 'N/A';
        try {
            const date = new Date(dateTimeString);
            return date.toLocaleString('ru-RU', {
                year: 'numeric', month: 'long', day: 'numeric',
                hour: '2-digit', minute: '2-digit'
            });
        } catch (e) {
            return dateTimeString;
        }
    };

    const getStationNameById = (stationId) => {
        if (!stationId || !stations || stations.length === 0) return 'Неизвестная станция';
        const station = stations.find(s => s.id === stationId);
        if (station) {
            return `${station.name}`;
        }
        return `Станция (ID: ...${stationId.slice(-6)})`;
    };

    return (
        <div className={styles.findRoutePage}>
            <h1>Поиск маршрутов</h1>
            <form onSubmit={handleSearch} className={styles.searchForm}>
                <div className={styles.formRow}>
                    <div className={styles.formGroup}>
                        <label htmlFor="fromStation">Станция отправления:</label>
                        <select
                            id="fromStation"
                            value={fromStationId}
                            onChange={(e) => setFromStationId(e.target.value)}
                            required
                        >
                            <option value="">Выберите станцию</option>
                            {stations.map((station) => (
                                <option key={station.id} value={station.id}>
                                    {station.name} ({station.region})
                                </option>
                            ))}
                        </select>
                    </div>
                    <div className={styles.formGroup}>
                        <label htmlFor="toStation">Станция прибытия:</label>
                        <select
                            id="toStation"
                            value={toStationId}
                            onChange={(e) => setToStationId(e.target.value)}
                            required
                        >
                            <option value="">Выберите станцию</option>
                            {stations.map((station) => (
                                <option key={station.id} value={station.id}>
                                    {station.name} ({station.region})
                                </option>
                            ))}
                        </select>
                    </div>
                </div>

                <div className={styles.formRow}>
                    <div className={styles.formGroup}>
                        <label htmlFor="departureDate">Дата отправления:</label>
                        <input
                            type="date"
                            id="departureDate"
                            value={departureDate}
                            onChange={(e) => setDepartureDate(e.target.value)}
                            required
                            min={new Date().toISOString().split('T')[0]} // Нельзя выбрать прошлую дату
                        />
                    </div>
                    <div className={`${styles.formGroup} ${styles.checkboxGroup}`}>
                        <input
                            type="checkbox"
                            id="isDirectRoute"
                            checked={isDirectRoute}
                            onChange={(e) => setIsDirectRoute(e.target.checked)}
                        />
                        <label htmlFor="isDirectRoute">Только прямой маршрут</label>
                    </div>
                </div>

                <button type="submit" disabled={isLoading} className={styles.searchButton}>
                    {isLoading ? 'Поиск...' : 'Найти маршруты'}
                </button>
            </form>

            {error && <p className={styles.errorMessage}>{error}</p>}

            {searchResults.length > 0 && (
                <div className={styles.resultsContainer}>
                    <h2>Найденные маршруты:</h2>
                    {searchResults.map((complexRoute, index) => (
                        <div key={index} className={styles.complexRouteCard}>
                            <h3>Маршрут {index + 1}</h3>
                            <p><strong>Время в пути:</strong> {formatDuration(complexRoute.totalDuration)}</p>
                            <p><strong>Отправление:</strong> {formatDateTime(complexRoute.departureDate)}</p>
                            <p><strong>Прибытие:</strong> {formatDateTime(complexRoute.arrivalDate)}</p>
                            <p><strong>Стоимость:</strong> от {complexRoute.minimalTotalCost?.toFixed(2)} до {complexRoute.maximumTotalCost?.toFixed(2)} руб.</p>

                            <h4>Участки маршрута:</h4>
                            {complexRoute.directRoutes.map((directRoute, drIndex) => (
                                <div key={directRoute.concreteRouteId || drIndex} className={styles.directRouteCard}>
                                    <p>Откуда: {getStationNameById(directRoute.fromStationId)}</p>
                                    <p>Куда: {getStationNameById(directRoute.toStationId)}</p>
                                    <p>Отправление: {formatDateTime(directRoute.departureDate)}</p>
                                    <p>Прибытие: {formatDateTime(directRoute.arrivalDate)}</p>
                                    <p>Время в пути: {formatDuration(directRoute.timeInTransit)}</p>
                                    <p>Стоимость: от {directRoute.minimalCost?.toFixed(2)} до {directRoute.maximumCost?.toFixed(2)} руб.</p>
                                    <p>Свободных мест: {directRoute.availableSeats}</p>
                                </div>
                            ))}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default FindRoutePage;