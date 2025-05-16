import { useState, useEffect, useCallback } from 'react';

export const useBookingManager = (selectedComplexRoute) => {
    const [bookingsData, setBookingsData] = useState([]);
    const [currentBookingSegmentIndex, setCurrentBookingSegmentIndex] = useState(0);

    useEffect(() => {
        if (selectedComplexRoute?.directRoutes?.length > 0) {
            setBookingsData(selectedComplexRoute.directRoutes.map(segment => ({
                concreteRouteId: segment.concreteRouteId,
                startSegmentNumber: segment.startSegmentNumber,
                endSegmentNumber: segment.endSegmentNumber,
                departureDate: segment.departureDate,
                arrivalDate: segment.arrivalDate,
                fromStationId: segment.fromStationId,
                toStationId: segment.toStationId,
                selectionsByCarriage: new Map(),
                isConfigurationDone: false
            })));
            setCurrentBookingSegmentIndex(0);
        } else {
            setBookingsData([]);
        }
    }, [selectedComplexRoute]);

    const saveSeatsForCarriageInSegment = useCallback((segmentIndex, carriageNum, seatsSet) => {
        setBookingsData(prevBookings => {
            const updatedBookings = [...prevBookings];
            if (!updatedBookings[segmentIndex]) return prevBookings;

            const currentSegmentBooking = { ...updatedBookings[segmentIndex] };
            const newSelectionsMap = new Map(currentSegmentBooking.selectionsByCarriage);
            newSelectionsMap.set(carriageNum, new Set(seatsSet));
            currentSegmentBooking.selectionsByCarriage = newSelectionsMap;
            updatedBookings[segmentIndex] = currentSegmentBooking;
            return updatedBookings;
        });
    }, []);

    const confirmSegmentAndProceed = useCallback(() => {
        setBookingsData(prevBookings => {
            const updatedBookings = [...prevBookings];
            if (!updatedBookings[currentBookingSegmentIndex]) return prevBookings;

            updatedBookings[currentBookingSegmentIndex] = {
                ...updatedBookings[currentBookingSegmentIndex],
                isConfigurationDone: true,
            };
            return updatedBookings;
        });

        if (currentBookingSegmentIndex < (selectedComplexRoute?.directRoutes?.length || 0) - 1) {
            setCurrentBookingSegmentIndex(prev => prev + 1);
            return false;
        } else {
            return true;
        }
    }, [currentBookingSegmentIndex, selectedComplexRoute?.directRoutes?.length]);

    const allSegmentsConfigured = bookingsData.length > 0 && bookingsData.every(b => b.isConfigurationDone);

    return {
        bookingsData,
        currentBookingSegmentIndex,
        saveSeatsForCarriageInSegment,
        confirmSegmentAndProceed,
        allSegmentsConfigured,
        setCurrentBookingSegmentIndex
    };
};