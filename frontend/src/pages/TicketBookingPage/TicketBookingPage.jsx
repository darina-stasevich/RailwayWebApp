import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { formatDateTime, formatDuration } from '../../utils/formatters.js';
import styles from './TicketBookingPage.module.css';

const TicketBookingPage = () => {
    const location = useLocation();
    const navigate = useNavigate();

    const selectedComplexRoute = location.state?.selectedComplexRoute;
    const fromStation = location.state?.fromStation;
    const toStation = location.state?.toStation;
    const stations = location.state?.stations;

    const [error, setError] = useState('');

    const getStationNameById = (stationId) => {
        if (!stationId || !stations || stations.length === 0) return 'Неизвестная станция';
        const station = stations.find(s => s.id === stationId);
        if (station) {
            return `${station.name}`;
        }
        return `Станция (ID: ...${stationId.slice(-6)})`;
    };

    useEffect(() => {
        if (!selectedComplexRoute || !stations) {
            console.warn("Данные о маршруте или станциях не были переданы. Возврат...");
            navigate('/find-route');
        }
    }, [selectedComplexRoute, stations, navigate]);

    // number of first direct route
    const [currentBookingSegmentIndex, setCurrentBookingSegmentIndex] = useState(0);
    // array of booking data for every direct route
    const [bookingsData, setBookingsData] = useState([]);

    // for current active direct route
    const [activeSegment_ShortCarriagesList, setActiveSegment_ShortCarriagesList] = useState(null);
    const [activeSegment_IsLoadingShortCarriages, setActiveSegment_IsLoadingShortCarriages] = useState(false);
    const [activeSegment_ViewingDetailedCarriage, setActiveSegment_ViewingDetailedCarriage] = useState(null);
    const [activeSegment_IsLoadingDetailedCarriage, setActiveSegment_IsLoadingDetailedCarriage] = useState(false);
    const [activeSegment_TemporarySeatsInViewingCarriage, setActiveSegment_TemporarySeatsInViewingCarriage] = useState(new Set());

    // bookingsData initialization
    useEffect(() => {
        if (selectedComplexRoute?.directRoutes?.length > 0) {
            setBookingsData(selectedComplexRoute.directRoutes.map(segment => ({
                concreteRouteId: segment.concreteRouteId,
                startSegmentNumber: segment.startSegmentNumber,
                endSegmentNumber: segment.endSegmentNumber,
                selectionsByCarriage: new Map(), // Map<carriageNum, Set<seatNum>>
                isConfigurationDone: false
            })));
        }
    }, [selectedComplexRoute]);

    // short summary
    const fetchShortCarriagesForSegment = async (segmentData) => {
        if (!segmentData) return;
        setActiveSegment_IsLoadingShortCarriages(true);
        setActiveSegment_ShortCarriagesList(null);
        setActiveSegment_ViewingDetailedCarriage(null);

        const requestBody = {
            concreteRouteId: segmentData.concreteRouteId,
            startSegmentNumber: segmentData.startSegmentNumber,
            endSegmentNumber: segmentData.endSegmentNumber,
        };
        console.log("Запрос ShortCarriageInfoDto с:", requestBody);
        const token = localStorage.getItem('token');

        if (!token) {
            setError('Ошибка: Пользователь не авторизован. Не удалось загрузить информацию о вагонах.');
            setActiveSegment_IsLoadingShortCarriages(false);
            navigate('/login');
            return;
        }
        try {
            const response = await fetch('http://localhost:5241/api/Carriages/summaries', {
                method: 'POST',
                body: JSON.stringify(requestBody),
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            if (!response.ok) {
                throw new Error(`Ошибка загрузки данных о вагонах: ${response.statusText}`);
            }
            setError('');
            const data = await response.json();
            setActiveSegment_ShortCarriagesList(data);
        } catch (error) {
            setError(error.message);
            console.error("Ошибка загрузки ShortCarriageInfoDto:", error);
        } finally {
            setActiveSegment_IsLoadingShortCarriages(false);
        }
    };

    // detailed info about carriage
    const fetchDetailedCarriageInfoForSegment = async (segmentData, carriageNumber) => {
        if (!segmentData || !carriageNumber) return;
        setActiveSegment_IsLoadingDetailedCarriage(true);
        setActiveSegment_ViewingDetailedCarriage(null);

        const requestBody = {
            concreteRouteId: segmentData.concreteRouteId,
            startSegmentNumber: segmentData.startSegmentNumber,
            endSegmentNumber: segmentData.endSegmentNumber,
            carriageNumber: carriageNumber,
        };
        console.log("Запрос DetailedCarriageInfoDto с:", requestBody);
        const token = localStorage.getItem('token');
        if (!token) {
            setError('Ошибка: Пользователь не авторизован. Не удалось загрузить информацию о вагоне.');
            setActiveSegment_IsLoadingDetailedCarriage(false);
            navigate('/login');
            return;
        }
        try {
            const response = await fetch('http://localhost:5241/api/Carriages/details', {
                method: 'POST',
                body: JSON.stringify(requestBody),
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            if (!response.ok) {
                throw new Error(`Ошибка загрузки данных о вагоне: ${response.statusText}`);
            }
            setError('');
            const data = await response.json();
            setActiveSegment_ViewingDetailedCarriage(data);
            const currentSegmentBooking = bookingsData[currentBookingSegmentIndex];
            const existingSelectionsForThisCarriage = currentSegmentBooking
                ? currentSegmentBooking.selectionsByCarriage.get(data.carriageNumber) || new Set()
                : new Set();
            setActiveSegment_TemporarySeatsInViewingCarriage(new Set(existingSelectionsForThisCarriage));
        } catch (error) {
            setError(error.message);
            console.error("Ошибка загрузки DetailedCarriageInfoDto:", error);
        } finally {
            setActiveSegment_IsLoadingDetailedCarriage(false);
        }
    };

    const handleToggleSeatInTemporarySelection = (seatNumber) => {
        setActiveSegment_TemporarySeatsInViewingCarriage(prevSeats => {
            const newSeats = new Set(prevSeats);
            if (newSeats.has(seatNumber)) {
                newSeats.delete(seatNumber);
            } else {
                newSeats.add(seatNumber);
            }
            return newSeats;
        });
    };

    const handleSaveSeatsForCarriage = () => {
        if (!activeSegment_ViewingDetailedCarriage) return;
        const carriageNum = activeSegment_ViewingDetailedCarriage.carriageNumber;

        setBookingsData(prevBookings => {
            const updatedBookings = [...prevBookings];
            const currentSegmentBooking = { ...updatedBookings[currentBookingSegmentIndex] };
            const newSelectionsMap = new Map(currentSegmentBooking.selectionsByCarriage);

            newSelectionsMap.set(carriageNum, new Set(activeSegment_TemporarySeatsInViewingCarriage));
            currentSegmentBooking.selectionsByCarriage = newSelectionsMap;
            updatedBookings[currentBookingSegmentIndex] = currentSegmentBooking;
            return updatedBookings;
        });

        setActiveSegment_ViewingDetailedCarriage(null);
    };

    const handleConfirmSegmentAndProceed = () => {
        setBookingsData(prevBookings => {
            const updatedBookings = [...prevBookings];
            updatedBookings[currentBookingSegmentIndex] = {
                ...updatedBookings[currentBookingSegmentIndex],
                isConfigurationDone: true,
            };
            return updatedBookings;
        });

        setActiveSegment_ShortCarriagesList(null);
        setActiveSegment_ViewingDetailedCarriage(null);
        setActiveSegment_TemporarySeatsInViewingCarriage(new Set());

        if (currentBookingSegmentIndex < selectedComplexRoute.directRoutes.length - 1) {
            setCurrentBookingSegmentIndex(prev => prev + 1);
        } else {
            console.log("Все сегменты сконфигурированы!", bookingsData);
            // Можно показать кнопку "Оформить заказ"
        }
    };

        // const currentActiveSegmentDataFromRoute = selectedComplexRoute?.directRoutes[currentBookingSegmentIndex];
        // const currentBookingForActiveSegment = bookingsData[currentBookingSegmentIndex];

    const handlePassengersDetailsClick = () => {
        navigate('/passengers-details', {
            state: {
                bookingsToProcess: bookingsData,
                complexRouteDetails: selectedComplexRoute,
                stations: stations
            }
        });
    };

    if (!selectedComplexRoute || !stations || bookingsData.length === 0) {
        return <p>Загрузка данных о маршруте или перенаправление...</p>;
    }

    const allSegmentsConfigured = bookingsData.every(b => b.isConfigurationDone);

    return (
        <div className={styles.ticketBookingPageContainer}>
            <h1>Бронирование билетов</h1>
            <h2>Маршрут: {fromStation} → {toStation}</h2>
            <p>{formatDateTime(selectedComplexRoute.departureDate)} - {formatDateTime(selectedComplexRoute.arrivalDate)}</p>
            <p>Время в пути: {formatDuration(selectedComplexRoute.totalDuration)}</p>
            <h3>
                Участки маршрута ({currentBookingSegmentIndex + 1} из {selectedComplexRoute.directRoutes.length})
            </h3>

            {selectedComplexRoute.directRoutes.map((segment, index) => {
                const bookingForThisSegment = bookingsData[index];
                const isActiveForBooking = index === currentBookingSegmentIndex && !bookingForThisSegment?.isConfigurationDone;

                let totalSelectedSeatsForSegment = 0;
                if (bookingForThisSegment?.selectionsByCarriage) {
                    bookingForThisSegment.selectionsByCarriage.forEach(setOfSeats => {
                        totalSelectedSeatsForSegment += setOfSeats.size;
                    });
                }


                return (
                    <div key={segment.concreteRouteId || index} className={`${styles.segmentBookingCard} ${isActiveForBooking ? styles.activeSegment : ''} ${bookingForThisSegment?.isConfigurationDone ? styles.completedSegment : ''}`}>
                        <h4>Сегмент {index + 1}: {getStationNameById(segment.fromStationId)} - {getStationNameById(segment.toStationId)}
                            {bookingForThisSegment?.isConfigurationDone && <span> (Выбрано мест: {totalSelectedSeatsForSegment})</span>}
                        </h4>
                        <p>Отправление: {formatDateTime(segment.departureDate)}</p>
                        <p>Прибытие:    {formatDateTime(segment.arrivalDate)}</p>

                        {isActiveForBooking && (
                            <>
                                {error !== '' && <p className={styles.errorMessage}>{error}</p>}
                                <button
                                    onClick={() => fetchShortCarriagesForSegment(bookingForThisSegment)}
                                    disabled={activeSegment_IsLoadingShortCarriages}
                                >
                                    {activeSegment_IsLoadingShortCarriages ? "Загрузка вагонов..." : "Показать доступные вагоны"}
                                </button>

                                {activeSegment_ShortCarriagesList && !activeSegment_ViewingDetailedCarriage && (
                                    <div className={styles.shortCarriageList}>
                                        <h5>Доступные вагоны для участка:</h5>
                                        {activeSegment_ShortCarriagesList.map(sc => {
                                            const selectedInThisCarriageCount = bookingForThisSegment?.selectionsByCarriage.get(sc.carriageNumber)?.size || 0;
                                            return (
                                                <div key={sc.carriageNumber} className={styles.shortCarriageItem}>
                                                    <p>Вагон №{sc.carriageNumber} ({sc.layoutIdentifier}), мест: {sc.availableSeats}, цена от: {sc.cost.toFixed(2)} руб.</p>
                                                    {selectedInThisCarriageCount > 0 && <p><small>Выбрано в этом вагоне: {selectedInThisCarriageCount}</small></p>}
                                                    <button onClick={() => fetchDetailedCarriageInfoForSegment(bookingForThisSegment, sc.carriageNumber)} disabled={activeSegment_IsLoadingDetailedCarriage}>
                                                        {activeSegment_IsLoadingDetailedCarriage && activeSegment_ViewingDetailedCarriage?.carriageNumber === sc.carriageNumber ? "Загрузка..." : "Выбрать/посмотреть места"}
                                                    </button>
                                                </div>
                                            )})}
                                    </div>
                                )}

                                {activeSegment_ViewingDetailedCarriage && (
                                    <div className={styles.detailedCarriageView}>
                                        <h5>Вагон №{activeSegment_ViewingDetailedCarriage.carriageNumber} ({activeSegment_ViewingDetailedCarriage.layoutIdentifier})</h5>
                                        <p>Всего мест: {activeSegment_ViewingDetailedCarriage.totalSeats}, доступно: {activeSegment_ViewingDetailedCarriage.availableSeats.length}, цена: {activeSegment_ViewingDetailedCarriage.cost.toFixed(2)}</p>
                                        <div className={styles.seatSchema}>
                                            {Array.from({ length: activeSegment_ViewingDetailedCarriage.totalSeats }, (_, i) => i + 1).map(seatNum => {
                                                const isAvailable = activeSegment_ViewingDetailedCarriage.availableSeats.includes(seatNum);
                                                const isSelectedTemporarily = activeSegment_TemporarySeatsInViewingCarriage.has(seatNum);
                                                return (
                                                    <button
                                                        key={seatNum}
                                                        className={`${styles.seatButton} ${isSelectedTemporarily ? styles.seatSelected : ''} ${!isAvailable ? styles.seatUnavailable : ''}`}
                                                        onClick={() => isAvailable && handleToggleSeatInTemporarySelection(seatNum)}
                                                        disabled={!isAvailable}
                                                        title={!isAvailable ? "Место занято" : `Место ${seatNum}`}
                                                    >
                                                        {seatNum}
                                                    </button>
                                                );
                                            })}
                                        </div>
                                        <button onClick={handleSaveSeatsForCarriage}>Сохранить выбор для этого вагона</button>
                                        <button onClick={() => setActiveSegment_ViewingDetailedCarriage(null)}>Назад к списку вагонов</button>
                                    </div>
                                )}

                                <hr />
                                <p>Всего выбрано мест для этого участка: {totalSelectedSeatsForSegment}</p>
                                <button onClick={handleConfirmSegmentAndProceed}>
                                    Подтвердить выбор для участка и перейти к следующему
                                </button>
                            </>
                        )}
                    </div>
                );
            })}

            {allSegmentsConfigured && (
                <div className={styles.finalBookingSection}>
                    <button className={styles.passengersDatabutton}
                        onClick={handlePassengersDetailsClick}>Перейти к оформлению</button>
                </div>
            )}
        </div>
    );
};

export default TicketBookingPage;