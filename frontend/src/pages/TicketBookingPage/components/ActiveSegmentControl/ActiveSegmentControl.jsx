import React, { useState, useEffect, useCallback } from 'react';
import { useCarriageData } from '../../hooks/useCarriageData.jsx';
import ShortCarriageInfoCard from '../ShortCarriageInfoCard/ShortCarriageInfoCard.jsx';
import DetailedCarriageInfoCard from '../DetailedCattiageInfoCard/DetailedCarriageInfoCard.jsx';
import styles from './ActiveSegmentControl.module.css';
import buttonStyles from '../../../../styles/ButtonStyles.module.css';

const ActiveSegmentControls = ({
                                   segmentBookingData,
                                   segmentApiParams,
                                   onSaveSeatsForCarriage,
                                   onConfirmSegmentAndProceed,
                                   token
                               }) => {
    const {
        shortCarriagesList,
        isLoadingShortCarriages,
        shortCarriagesError,
        fetchShortCarriages,
        detailedCarriage,
        isLoadingDetailedCarriage,
        detailedCarriageError,
        fetchDetailedCarriage,
        clearDetailedCarriageView,
        clearAllCarriageData
    } = useCarriageData(segmentApiParams, token);

    const [temporarySeatsInViewingCarriage, setTemporarySeatsInViewingCarriage] = useState(new Set());
    const [lastLoadedDetailedCarriageNumber, setLastLoadedDetailedCarriageNumber] = useState(null);


    useEffect(() => {
        // Clear carriage data when the segment to control changes
        clearAllCarriageData();
        setTemporarySeatsInViewingCarriage(new Set());
    }, [segmentApiParams, clearAllCarriageData]);


    const handleFetchDetailedCarriage = useCallback(async (carriageNumber) => {
        await fetchDetailedCarriage(carriageNumber);
        setLastLoadedDetailedCarriageNumber(carriageNumber);
    }, [fetchDetailedCarriage]);

    useEffect(() => {
        if (detailedCarriage && segmentBookingData) {
            const existingSelections = segmentBookingData.selectionsByCarriage.get(detailedCarriage.carriageNumber) || new Set();
            setTemporarySeatsInViewingCarriage(new Set(existingSelections));
        }
    }, [detailedCarriage, segmentBookingData, lastLoadedDetailedCarriageNumber]);


    const handleToggleSeatInTemporarySelection = (seatNumber) => {
        setTemporarySeatsInViewingCarriage(prevSeats => {
            const newSeats = new Set(prevSeats);
            if (newSeats.has(seatNumber)) newSeats.delete(seatNumber);
            else newSeats.add(seatNumber);
            return newSeats;
        });
    };

    const handleSaveSeats = () => {
        if (!detailedCarriage) return;
        onSaveSeatsForCarriage(detailedCarriage.carriageNumber, temporarySeatsInViewingCarriage);
        clearDetailedCarriageView();
    };

    const handleConfirmAndProceed = () => {
        onConfirmSegmentAndProceed();
        clearAllCarriageData();
        setTemporarySeatsInViewingCarriage(new Set());
    };

    let totalSelectedSeatsForThisSegment = 0;
    if (segmentBookingData?.selectionsByCarriage) {
        segmentBookingData.selectionsByCarriage.forEach(setOfSeats => {
            totalSelectedSeatsForThisSegment += setOfSeats.size;
        });
    }

    const apiError = shortCarriagesError || detailedCarriageError;

    return (
        <>
            {apiError && <p className={styles.errorMessage}>{apiError}</p>}
            <button className={buttonStyles.showCarriagesButton}
                onClick={fetchShortCarriages}
                disabled={isLoadingShortCarriages}
            >
                {isLoadingShortCarriages ? "Загрузка вагонов..." : "Показать доступные вагоны"}
            </button>

            {shortCarriagesList && !detailedCarriage && (
                <div className={styles.shortCarriageList}>
                    <h5>Доступные вагоны для участка:</h5>
                    {shortCarriagesList.map(sc => {
                        const selectedInThisCarriageCount = segmentBookingData?.selectionsByCarriage.get(sc.carriageNumber)?.size || 0;
                        return (
                            <ShortCarriageInfoCard
                                key={sc.carriageNumber}
                                carriageSummary={sc}
                                selectedInThisCarriageCount={selectedInThisCarriageCount}
                                onViewDetails={() => handleFetchDetailedCarriage(sc.carriageNumber)}
                                isLoadingDetails={isLoadingDetailedCarriage && detailedCarriage?.carriageNumber === sc.carriageNumber}
                            />
                        );
                    })}
                </div>
            )}

            {detailedCarriage && (
                <DetailedCarriageInfoCard
                    carriageDetails={detailedCarriage}
                    temporarySeats={temporarySeatsInViewingCarriage}
                    onToggleSeat={handleToggleSeatInTemporarySelection}
                    onSave={handleSaveSeats}
                    onBackToList={clearDetailedCarriageView}
                />
            )}

            <hr />
            <p>Всего выбрано мест для этого участка: {totalSelectedSeatsForThisSegment}</p>
            <button className={buttonStyles.confirmButton}
                    onClick={handleConfirmAndProceed}
                    disabled={totalSelectedSeatsForThisSegment === 0}>
                Подтвердить выбор для участка и перейти к следующему
            </button>
        </>
    );
};

export default ActiveSegmentControls;