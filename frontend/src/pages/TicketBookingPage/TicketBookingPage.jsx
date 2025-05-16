import React, { useState, useEffect, useCallback } from 'react';
import styles from './TicketBookingPage.module.css';

import { useLocation, useNavigate } from 'react-router-dom';
import { formatDateTime, formatDuration } from '../../utils/formatters.js';
import { useBookingManager } from './hooks/useBookingManager';

import RouteInfoDisplay from './components/RouteInfoHeader/RouteInfoDisplay.jsx';
import SegmentCard from './components/SegmentCard/SegmentCard.jsx';
import ActiveSegmentControls from './components/ActiveSegmentControl/ActiveSegmentControl.jsx';

const TicketBookingPage = () => {
    const location = useLocation();
    const navigate = useNavigate();

    const selectedComplexRoute = location.state?.selectedComplexRoute;
    const fromStationName = location.state?.fromStation;
    const toStationName = location.state?.toStation;
    const stations = location.state?.stations;

    const {
        bookingsData,
        currentBookingSegmentIndex,
        saveSeatsForCarriageInSegment,
        confirmSegmentAndProceed,
        allSegmentsConfigured
    } = useBookingManager(selectedComplexRoute);

    const [token, setToken] = useState(null);

    useEffect(() => {
        const storedToken = localStorage.getItem('token');
        if (storedToken) {
            setToken(storedToken);
        } else {
            console.warn("Пользователь не авторизован, перенаправление на логин.");
            navigate('/login');
        }
    }, [navigate]);


    const getStationNameById = useCallback((stationId) => {
        if (!stationId || !stations || stations.length === 0) return 'Неизвестная станция';
        const station = stations.find(s => s.id === stationId);
        return station ? station.name : `Станция (ID: ...${stationId.slice(-6)})`;
    }, [stations]);

    useEffect(() => {
        if (!selectedComplexRoute || !stations) {
            console.warn("Данные о маршруте или станциях не были переданы. Возврат на /find-route");
            navigate('/find-route');
        }
    }, [selectedComplexRoute, stations, navigate]);


    const handleSaveSeats = (carriageNum, seatsSet) => {
        saveSeatsForCarriageInSegment(currentBookingSegmentIndex, carriageNum, seatsSet);
    };

    const handleConfirmAndProceed = () => {
        const allDone = confirmSegmentAndProceed();
        if (allDone) {
            console.log("Все места выбраны!", bookingsData);
        }
    };

    const handlePassengersDetailsClick = () => {
        navigate('/passengers-details', {
            state: {
                bookingToProcess: bookingsData.flatMap(segment => {
                    const seatsForSegment = [];
                    segment.selectionsByCarriage.forEach((seatsSet, carriageNumber) => {
                        seatsSet.forEach(seatNumber => {
                            seatsForSegment.push({
                                concreteRouteId: segment.concreteRouteId,
                                startSegmentNumber: segment.startSegmentNumber,
                                endSegmentNumber: segment.endSegmentNumber,
                                carriageNumber: carriageNumber,
                                seatNumber: seatNumber,
                                hasBedLinenSet: false,
                                passengerData: null
                            });
                        });
                    });
                    return seatsForSegment;
                }),
                complexRouteDetails: selectedComplexRoute,
                stationsList: stations
            }
        });
    };

    if (!selectedComplexRoute || !stations || bookingsData.length === 0 || !token) {
        return <p>Загрузка данных о маршруте, проверка авторизации или перенаправление...</p>;
    }

    const currentActiveSegmentForControls = selectedComplexRoute.directRoutes[currentBookingSegmentIndex];
    const currentBookingDataForControls = bookingsData[currentBookingSegmentIndex];

    return (
        <div className={styles.ticketBookingPageContainer}>
            <h1>Бронирование билетов</h1>
            <RouteInfoDisplay
                fromStation={fromStationName}
                toStation={toStationName}
                selectedComplexRoute={selectedComplexRoute}
                formatDateTime={formatDateTime}
                formatDuration={formatDuration}
            />

            <h3>
                Участки маршрута ({currentBookingSegmentIndex + 1} из {selectedComplexRoute.directRoutes.length})
            </h3>

            {selectedComplexRoute.directRoutes.map((segment, index) => {
                const bookingForThisSegment = bookingsData[index];
                const isActiveForBooking = index === currentBookingSegmentIndex && !bookingForThisSegment?.isConfigurationDone;

                return (
                    <SegmentCard
                        key={segment.concreteRouteId || index}
                        segment={segment}
                        index={index}
                        bookingForThisSegment={bookingForThisSegment}
                        isActiveForBooking={isActiveForBooking}
                        getStationNameById={getStationNameById}
                        formatDateTime={formatDateTime}
                    >
                        {isActiveForBooking && currentActiveSegmentForControls && currentBookingDataForControls && (
                            <ActiveSegmentControls
                                segmentBookingData={currentBookingDataForControls}
                                segmentApiParams={{
                                    concreteRouteId: currentActiveSegmentForControls.concreteRouteId,
                                    startSegmentNumber: currentActiveSegmentForControls.startSegmentNumber,
                                    endSegmentNumber: currentActiveSegmentForControls.endSegmentNumber
                                }}
                                onSaveSeatsForCarriage={handleSaveSeats}
                                onConfirmSegmentAndProceed={handleConfirmAndProceed}
                                token={token}
                            />
                        )}
                    </SegmentCard>
                );
            })}

            {allSegmentsConfigured && (
                <div className={styles.finalBookingSection}>
                    <button className={styles.passengersDataButton}
                            onClick={handlePassengersDetailsClick}>Перейти к оформлению</button>
                </div>
            )}
        </div>
    );
};

export default TicketBookingPage;