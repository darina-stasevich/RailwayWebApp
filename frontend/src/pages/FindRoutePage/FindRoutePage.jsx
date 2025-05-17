import React, { useState, useEffect } from 'react';
import styles from './FindRoutePage.module.css';
import { useNavigate } from 'react-router-dom';
import ComplexRouteCard from "./Cards/ComplexRouteCard.jsx";
import SearchForm from "./Cards/SearchForm.jsx";
import ScheduleModal from "./Schedule/ScheduleModal.jsx";
import {formatDateTime, formatDuration} from "../../utils/formatters.js";
import { useStations } from '../../contexts/StationsContext.jsx';

const API_BASE_URL = 'http://localhost:5241/api';

const FindRoutePage = () => {
    const { stations, isLoadingStations, errorStations, getStationNameById } = useStations();

    const [fromStationId, setFromStationId] = useState('');
    const [toStationId, setToStationId] = useState('');
    const [departureDate, setDepartureDate] = useState(() => {
        const today = new Date();
        return today.toISOString().split('T')[0];
    });

    // for searching
    const [isDirectRoute, setIsDirectRoute] = useState(true);
    const [searchResults, setSearchResults] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');

    const navigate = useNavigate();

    // for route schedule
    const [scheduleData, setScheduleData] = useState(null);
    const [isScheduleModalOpen, setIsScheduleModalOpen] = useState(false);
    const [isLoadingSchedule, setIsLoadingSchedule] = useState(false);
    const [scheduleError, setScheduleError] = useState('');

    useEffect(() => {
        if (!isLoadingStations && stations && stations.length > 0) {
            if (!fromStationId && stations[0]) setFromStationId(stations[0].id);
            if (!toStationId && stations[1]) setToStationId(stations[1].id);
        }
    }, [stations, isLoadingStations, fromStationId, toStationId]);

    // for route search
    const handleSearch = async (e) => {
        e.preventDefault();
        setError('');
        setSearchResults([]);

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
            const response = await fetch(`${API_BASE_URL}/Routes/search`, {
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

    // show schedule for route
    const handleShowSchedule = async (concreteRouteId) => {
        setScheduleData(null);
        setScheduleError('');
        setIsLoadingSchedule(true);
        setIsScheduleModalOpen(true);

        const token = localStorage.getItem('token');
        if (!token) {
            setScheduleError('Ошибка: Пользователь не авторизован для просмотра расписания.');
            setIsLoadingSchedule(false);
            setIsScheduleModalOpen(false);
            navigate("/login");
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/Schedules/${concreteRouteId}`, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error('Ошибка авторизации при загрузке расписания.');
                }
                const errorData = await response.json().catch(() => ({ message: `Ошибка загрузки расписания: ${response.statusText}` }));
                throw new Error(errorData.message || errorData.title || `Ошибка ${response.status}`);
            }
            const data = await response.json();
            setScheduleData(data);
        } catch (err) {
            console.error("Ошибка при загрузке расписания:", err);
            setScheduleError(err.message);
        } finally {
            setIsLoadingSchedule(false);
        }
    };

    if (isLoadingStations) {
        return <div className={styles.loadingMessage}>Загрузка данных станций...</div>;
    }

    if (errorStations) {
        return <div className={styles.errorMessage}>Ошибка загрузки станций: {errorStations}. Пожалуйста, обновите страницу или попробуйте позже.</div>;
    }

    return (
        <div className={styles.findRoutePage}>
            <h1>Поиск маршрутов</h1>
            <SearchForm
                stations={stations}
                fromStationId={fromStationId}
                onFromStationChange={setFromStationId}
                toStationId={toStationId}
                onToStationChange={setToStationId}
                departureDate={departureDate}
                onDepartureDateChange={setDepartureDate}
                isDirectRoute={isDirectRoute}
                onIsDirectRouteChange={setIsDirectRoute}
                onSubmit={handleSearch}
                isLoading={isLoading}>
            </SearchForm>

            {error && <p className={styles.errorMessage}>{error}</p>}

            {searchResults.length > 0 && (
                <div className={styles.resultsContainer}>
                    <h2>Найденные маршруты:</h2>
                    {searchResults.map((complexRoute, index) => (
                        <ComplexRouteCard
                            key={index}
                            routeIndex={index}
                            complexRoute={complexRoute}
                            formatDuration={formatDuration}
                            formatDateTime={formatDateTime}
                            getStationNameById={getStationNameById}
                            onShowSchedule={handleShowSchedule}
                            stations={stations}>
                        </ComplexRouteCard>
                    ))}
                </div>
            )}

            <ScheduleModal
                isOpen={isScheduleModalOpen}
                onClose={() => setIsScheduleModalOpen(false)}
                scheduleData={scheduleData}
                isLoading={isLoadingSchedule}
                error={scheduleError}
                formatDateTime={formatDateTime}>
            </ScheduleModal>
        </div>
    );
};

export default FindRoutePage;