import React, { useState, useEffect } from 'react';
import styles from './FindRoutePage.module.css';
import { useNavigate } from 'react-router-dom';
import ComplexRouteCard from "./ComplexRouteCard.jsx";
import SearchForm from "./SearchForm.jsx";
import ScheduleModal from "./Schedule/ScheduleModal.jsx";

const FindRoutePage = () => {
    // for route search: fromStation, toStation, departureDate, isDirectRoute
    const [stations, setStations] = useState([]);
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

    // for stations fetching
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
            const response = await fetch(`http://localhost:5241/api/Schedules/${concreteRouteId}`, {
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

    // beautiful timeSpan
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

    // beautiful dateTime
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

    // get station name by id
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
                            onShowSchedule={handleShowSchedule}>
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