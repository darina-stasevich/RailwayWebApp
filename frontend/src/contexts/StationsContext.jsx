import React, {createContext, useState, useEffect, useContext, useCallback} from 'react';
import {useNavigate} from "react-router-dom";

const StationsContext = createContext();

export const useStations = () => {
    const context = useContext(StationsContext);
    if (context === undefined) {
        throw new Error('useStations must be used within a StationsProvider');
    }
    return context;
};

export const StationsProvider = ({ children }) => {
    const [stations, setStations] = useState([]);
    const [isLoadingStations, setIsLoadingStations] = useState(true);
    const [errorStations, setErrorStations] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        const loadStations = async () => {
            setIsLoadingStations(true);
            setErrorStations(null);

            const token = localStorage.getItem('token');

            const headers = {
                'Content-Type': 'application/json',
            };

            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            } else {
                navigate('/login');
            }

            try {
                const response = await fetch('http://localhost:5241/api/Stations', {
                    method: 'GET',
                    headers: headers,
                });

                if (!response.ok) {
                    let errorDataMessage = `HTTP error ${response.status}`;
                    try {
                        const errorData = await response.json();
                        errorDataMessage = errorData.message || errorData.title || JSON.stringify(errorData);
                    } catch (e) {
                        errorDataMessage = await response.text() || errorDataMessage;
                    }
                    throw new Error(`Failed to fetch stations: ${errorDataMessage}`);
                }

                const data = await response.json();
                setStations(data || []);
                console.log("StationsContext: Stations loaded successfully", data);
            } catch (err) {
                setErrorStations(err.message);
                console.error("Error in StationsProvider loading stations:", err);
            } finally {
                setIsLoadingStations(false);
            }
        };

        loadStations();
    }, []);

    const getStationNameById = useCallback((stationId) => {
        if (isLoadingStations) return 'Загрузка станций...';
        if (errorStations) return 'Ошибка загрузки станций';
        if (!stations || stations.length === 0) return 'Список станций пуст';
        if (!stationId) return 'Станция не указана';

        const station = stations.find(s => String(s.id) === String(stationId));
        return station ? station.name : `ID ${String(stationId).slice(-6)}`;
    }, [stations, isLoadingStations, errorStations]);

    return (
        <StationsContext.Provider value={{ stations, isLoadingStations, errorStations, getStationNameById }}>
            {children}
        </StationsContext.Provider>
    );
};